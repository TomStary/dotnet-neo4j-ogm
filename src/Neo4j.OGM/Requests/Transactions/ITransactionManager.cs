using System.Transactions;

namespace Neo4j.OGM.Requests.Transactions;

public interface ITransactionManager
{
    ITransaction CreateTransaction();
    ITransaction CurrentTransaction();
    void Commit(ITransaction transaction);
    void Rollback(ITransaction transaction);
}
