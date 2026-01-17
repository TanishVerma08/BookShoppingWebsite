using EcommProject.Data;
using EcommProject.DataAccess.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace EcommProject.DataAccess.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        internal DbSet<T> dbSet;
        public Repository(ApplicationDbContext context)
        {
            _context = context;
            dbSet = _context.Set<T>();
        }
        public void Add(T entity)
        {
            dbSet.Add(entity);
        }

        public T FirstOrDefault(Expression<Func<T, bool>> filter = null, string includeProperties = null) 
            //Isme ek hi record ayega to sorting ki need nhi hai
        {
            IQueryable<T> query = dbSet;
            if(filter != null)
                query = query.Where(filter);     
            if (includeProperties != null)
            {
                foreach (var includeProp in includeProperties.Split(new[]{','},StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp);
                }
            }
            return query.FirstOrDefault();
        }

        public T Get(int id)
        {
            return dbSet.Find(id);
        }

        public IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, //For Filter Condition
            Func<IQueryable<T>, IOrderedQueryable<T>> orderby = null, // For Sorting
            string includeProperties = null) // For Multiple Tables
        {
            IQueryable<T> // ye Ek interface hai query krne ke liye
                query = dbSet; // query hmne ek variable liya hai jisme dbset pass krdiya hai
            if (filter != null)  //Filter check krenge 
                query = query.Where(filter); //Agar filter hoga to apply ho jayega [means query mai vo data ayega jo filter m hoga
                                             //this is for specific data from single data]

            if(includeProperties != null) //This is for multiple tables [Category, CoverType i.e. Multiple data]
            {
                foreach (var includeProp in includeProperties.Split(new[] {','},StringSplitOptions.RemoveEmptyEntries)) 
                    //Pata Kese chlega multiple tables ka isliye array bnakr split by comma krdia hai [Split hota hai array ke liye]
                    //Remove Empty Entries for preventing from null data to be selected
                {
                    query = query.Include(includeProp); //[Its a join]
                                                        //Include hota hai multiple tables se data fetch krne ke liye
                }
            }
            if (orderby != null)
                return orderby(query).ToList();
            return query.ToList();
        }

        public void Remove(T entity)
        {
            dbSet.Remove(entity);
        }

        public void Remove(int id)
        {
            dbSet.Remove(Get(id));
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            dbSet.RemoveRange(entities);
        }

        public void Update(T entity)
        {
            _context.ChangeTracker.Clear();
            dbSet.Update(entity);
        }
    }
}
