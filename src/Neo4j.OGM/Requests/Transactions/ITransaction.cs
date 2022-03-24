namespace Neo4j.OGM.Requests.Transactions;

public interface ITransaction
{
    void Commit();
    void Rollback();
    Status GetStatus();

    enum Status
    {
        OPEN,
        PENDING,
        ROLLEDBACK,
        COMMITTED,
        CLOSED,
        ROLEBACK_PENDING,
        COMMIT_PENDING,
    }
}
