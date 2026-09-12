using System;
using System.Collections.Generic;
using System.Text;

namespace Vexora.Domain.sharedKernel.BaseEntities;

public abstract class BaseEntity
{
    public long Id { get; protected set; }
    public DateTimeOffset CreateAt { get; internal set; }
    public DateTimeOffset UpdateAt { get; internal set; }
    public bool IsDelete { get; private set; }=false;

    #region Functions

    public void Update()
    {
        UpdateAt = DateTimeOffset.Now;
    }

    #endregion
}
