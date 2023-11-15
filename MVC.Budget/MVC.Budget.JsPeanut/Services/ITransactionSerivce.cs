using Microsoft.AspNetCore.Mvc;
using MVC.Budget.JsPeanut.Data;
using System.Transactions;

namespace MVC.Budget.JsPeanut.Services
{
    public interface ITransactionSerivce
    {
        public List<Models.Transaction> GetAllTransactions();
        public Models.Transaction GetTransaction(int id);
        public void AddTransaction(Models.Transaction transaction);
        public void DeleteTransaction(Models.Transaction transaction);
        public void UpdateTransaction(Models.Transaction transaction);
    }
}
