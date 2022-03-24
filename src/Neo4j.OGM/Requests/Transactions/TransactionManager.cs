namespace Neo4j.OGM.Requests.Transactions;

public class TransactionManager : ITransactionManager
{
    public void Commit(ITransaction transaction)
    {
        throw new NotImplementedException();
    }

    public ITransaction CreateTransaction()
    {
        throw new NotImplementedException();
    }

    public ITransaction CurrentTransaction()
    {
        throw new NotImplementedException();
    }

    public void Rollback(ITransaction transaction)
    {
        throw new NotImplementedException();
    }
}
