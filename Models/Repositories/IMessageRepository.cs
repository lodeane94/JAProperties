﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SS.Models.Repositories
{
    public interface IMessageRepository : IRepository<Message>
    {
        IEnumerable<Message> GetAllMsgsForID(Guid Id);
    }
}