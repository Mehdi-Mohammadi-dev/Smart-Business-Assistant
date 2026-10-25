namespace Vexora.Domain.sharedKernel.BaseEntities;

public abstract class BaseEntity
{
    public long Id { get; protected set; }
    public DateTimeOffset CreateAt { get; internal set; }
    public DateTimeOffset UpdateAt { get; internal set; }
    public bool IsDelete { get; private set; }

    public DateTimeOffset? DeletedAt { get; private set; }

    public void SoftDelete()
    {
        if (IsDelete)
            return;

        var now = DateTimeOffset.UtcNow;

        IsDelete = true;
        DeletedAt = now;
        UpdateAt = now;
    }
}
