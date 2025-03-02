using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Bencodex;
using Libplanet.Action;
using Libplanet.Blockchain.Policies;
using Libplanet.Blocks;
using Libplanet.Crypto;
using Libplanet.Net;

namespace Libplanet.Explorer.Executable
{
    public class Options
    {
        private static readonly Codec Codec = new Codec();

        public Options(
            bool debug,
            string host,
            int port,
            int blockIntervalMilliseconds,
            long minimumDifficulty,
            int difficultyBoundDivisor,
            int workers,
            string appProtocolVersionToken,
            string mysqlServer,
            uint? mysqlPort,
            string mysqlUsername,
            string mysqlPassword,
            string mysqlDatabase,
            int maxTransactionsPerBlock,
            int maxBlockBytes,
            int maxGenesisBytes,
            IEnumerable<string> seedStrings,
            string iceServerUrl,
            string storePath,
            string storeType,
            string genesisBlockPath
        )
        {
            Debug = debug;
            Host = host;
            Port = port;
            BlockIntervalMilliseconds = blockIntervalMilliseconds;
            MinimumDifficulty = minimumDifficulty;
            DifficultyBoundDivisor = difficultyBoundDivisor;
            Workers = workers;
            AppProtocolVersionToken = appProtocolVersionToken;
            MySQLServer = mysqlServer;
            MySQLPort = mysqlPort;
            MySQLUsername = mysqlUsername;
            MySQLPassword = mysqlPassword;
            MySQLDatabase = mysqlDatabase;
            MaxTransactionsPerBlock = maxTransactionsPerBlock;
            MaxBlockBytes = maxBlockBytes;
            MaxGenesisBytes = maxGenesisBytes;
            SeedStrings = seedStrings;
            IceServerUrl = iceServerUrl;
            StorePath = storePath;
            StoreType = storeType;
            GenesisBlockPath = genesisBlockPath;
        }

        public bool Debug { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public int BlockIntervalMilliseconds { get; set; }

        public long MinimumDifficulty { get; set; }

        public int DifficultyBoundDivisor { get; set; }

        public int Workers { get; set; }

        public string AppProtocolVersionToken { get; set; }

        public string MySQLServer { get; set; }

        public uint? MySQLPort { get; set; }

        public string MySQLUsername { get; set; }

        public string MySQLPassword { get; set; }

        public string MySQLDatabase { get; set; }

        public int MaxTransactionsPerBlock { get; set; }

        public int MaxBlockBytes { get; set; }

        public int MaxGenesisBytes { get; set; }

        public IEnumerable<string> SeedStrings
        {
            get
            {
                return Seeds?.Select(seed => $"{ByteUtil.Hex(seed.PublicKey.Format(true))}," +
                                             $"{seed.EndPoint.Host},{seed.EndPoint.Port}");
            }

            set
            {
                Seeds = value?.Select(str =>
                {
                    string[] parts = str.Split(',');
                    if (parts.Length != 3)
                    {
                        throw new FormatException(
                            $"A seed must be a command-separated triple. {str}");
                    }

                    byte[] pubkeyBytes = ByteUtil.ParseHex(parts[0]);
                    var pubkey = new PublicKey(pubkeyBytes);
                    var endpoint = new DnsEndPoint(parts[1], int.Parse(parts[2]));
                    return new BoundPeer(pubkey, endpoint);
                });
            }
        }

        public IEnumerable<BoundPeer> Seeds { get; set; }

        public string IceServerUrl
        {
            get => IceServer is null ? null : IceServer.Url.ToString();
            set => IceServer = value is null ? null : new IceServer(value);
        }

        public IceServer IceServer { get; set; }

        public string StorePath { get; set; }

        public string StoreType { get; set; }

        public string GenesisBlockPath { get; set; }

        internal Block<NullAction> GetGenesisBlock(IBlockPolicy<NullAction> policy)
        {
            var uri = new Uri(GenesisBlockPath);
            using (var client = new WebClient())
            {
                var serialized = client.DownloadData(uri);
                var dict = (Bencodex.Types.Dictionary)Codec.Decode(serialized);
                return BlockMarshaler.UnmarshalBlock<NullAction>(policy.GetHashAlgorithm, dict);
            }
        }
    }
}
