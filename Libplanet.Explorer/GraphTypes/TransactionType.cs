#nullable disable
using System.Linq;
using GraphQL;
using GraphQL.Types;
using Libplanet.Action;
using Libplanet.Blockchain;
using Libplanet.Blocks;
using Libplanet.Explorer.Interfaces;
using Libplanet.Explorer.Store;
using Libplanet.Tx;

namespace Libplanet.Explorer.GraphTypes
{
    public class TransactionType<T> : ObjectGraphType<Transaction<T>>
        where T : IAction, new()
    {
        public TransactionType()
        {
            Field<NonNullGraphType<IdGraphType>>(
            name: "Id",
            description: "A unique identifier derived from this transaction content.",
            resolve: ctx => ctx.Source.Id.ToString());
            Field<NonNullGraphType<LongGraphType>>(
            name: "Nonce",
            description: "The number of previous transactions committed by the signer of this tx.",
            resolve: x => x.Source.Nonce
            );
            Field(
            type: typeof(NonNullGraphType<AddressType>),
            name: "Signer",
            description: "An address of the account who signed this transaction.",
            resolve: x => x.Source.Signer
            );
            Field<NonNullGraphType<ByteStringType>>(
            name: "PublicKey",
            description: "A PublicKey of the account who signed this transaction.",
            resolve: ctx => ctx.Source.PublicKey.Format(true)
            );
            Field(
            type: typeof(NonNullGraphType<ListGraphType<NonNullGraphType<AddressType>>>),
            name: "UpdatedAddresses",
            description: "Addresses whose states were affected by Actions.",
            resolve: x => x.Source.UpdatedAddresses
            );
            Field(
            type: typeof(NonNullGraphType<ByteStringType>),
            name: "Signature",
            description: "A digital signature of the content of this transaction.",
            resolve: x => x.Source.Signature
            );
            Field(
            type: typeof(NonNullGraphType<DateTimeOffsetGraphType>),
            name: "Timestamp",
            description: "The time this transaction was created and signed.",
            resolve: x => x.Source.Timestamp
            );
            Field<NonNullGraphType<ListGraphType<NonNullGraphType<ActionType<T>>>>>(
                name: "Actions",
                description: "A list of actions in this transaction."
                );

            // The block including the transaction. - Only RichStore supports.
            Field<ListGraphType<NonNullGraphType<BlockType<T>>>>(
                name: "BlockRef",
                description: "The block including the transaction.",
                resolve: ctx =>
                {
                    // FIXME: use context with DI.
                    const string storeKey = nameof(IBlockChainContext<T>.Store);
                    const string blockChainKey = nameof(IBlockChainContext<T>.BlockChain);
                    if (ctx.UserContext[storeKey] is IRichStore richStore &&
                        ctx.UserContext[blockChainKey] is BlockChain<T> chain)
                    {
                        HashAlgorithmGetter hashAlgorithmGetter = chain.Policy.GetHashAlgorithm;
                        return richStore
                            .IterateTxReferences(ctx.Source.Id)
                            .Select(r => richStore.GetBlock<T>(hashAlgorithmGetter, r.Item2));
                    }
                    else
                    {
                        var exceptionMessage =
                            $"This feature 'BlockRef' needs{nameof(IRichStore)} implementation";
                        ctx.Errors.Add(new ExecutionError(exceptionMessage));
                        return null;
                    }
                });

            Name = "Transaction";
        }
    }
}
