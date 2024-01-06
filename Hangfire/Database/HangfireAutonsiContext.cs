using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ESD_EDI_BE.Hangfire.Models;
using Microsoft.EntityFrameworkCore;

namespace ESD_EDI_BE.Hangfire.Database
{
    public class HangfireAutonsiContext : DbContext
    {
        public HangfireAutonsiContext(DbContextOptions<HangfireAutonsiContext> options) : base(options)
        {
        }

        public virtual DbSet<Token> Token { get; set; }
        public virtual DbSet<Document> Document { get; set; }
    }
}