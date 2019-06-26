using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Ituran.Framework.Data.Configuration;
using Ituran.Framework.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsurerServices.CommonBusiness.Data
{
	public class Context:DbContext
	{
		private IDbConnection Conexao { get; set; }
		private int? CommandTimeout { get; set; }
		private int? ReaderTimeout { get; set; }
		public DbSet<APOLICE_PROCESSAMENTO> PolicyProcess { get; set; }
		public DbSet<APOLICE_PROCESSAMENTO_DADOS> PolicyProcessData { get; set; }
		public DbSet<SEGURADORAICSCONFIG> IcsIsurerConfig { get; set; }

		public IEnumerable<TEntity> Procedure<TEntity>(string nomeProcedure, object objeto)
		{
			if (Conexao == null)
				Conexao = new SqlConnection(Database.GetDbConnection().ConnectionString);

			if (Conexao.State != ConnectionState.Open)
				Conexao.Open();

			return Conexao.Query<TEntity>($"{nomeProcedure}", objeto, commandType: CommandType.StoredProcedure, commandTimeout: CommandTimeout ?? int.MaxValue);
		}

		public IEnumerable<TEntity> Query<TEntity>(string query)
		{
			if (Conexao == null)
				Conexao = new SqlConnection(Database.GetDbConnection().ConnectionString);

			if (Conexao.State != ConnectionState.Open)
				Conexao.Open();

			return Conexao.Query<TEntity>($"{query}", commandType: CommandType.Text, commandTimeout: CommandTimeout ?? int.MaxValue);
		}

		public int Execute(string query)
		{
			if (Conexao == null)
				Conexao = new SqlConnection(Database.GetDbConnection().ConnectionString);

			if (Conexao.State != ConnectionState.Open)
				Conexao.Open();

			return Conexao.Execute($"{query}", commandType: CommandType.Text, commandTimeout: CommandTimeout ?? int.MaxValue);
		}

		public void Rollback()
		{
			base.ChangeTracker.Entries().ToList().ForEach(entry => entry.State = EntityState.Unchanged);
		}

		public Context(DbContextOptions<Context> options, int? commandTimeout = null, int? readerTimeout = null) : base(options)
		{
			CommandTimeout = commandTimeout;
			ReaderTimeout = readerTimeout;
		}

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new ApoliceProcessamentoEntityConfiguration());
			modelBuilder.ApplyConfiguration(new ApoliceProcessamentoDadosEntityConfiguration());
			modelBuilder.ApplyConfiguration(new SeguradoraIcsConfigEntityTypeConfiguration());
		}
	}
}
