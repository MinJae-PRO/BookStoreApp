using BookStoreApp.Data;
using BookStoreApp.Tests.Utilities;

namespace BookStoreApp.Tests.Utilities
{
    public abstract class DatabaseTestBase : IDisposable
    {
        protected readonly AppDbContext Context;
        protected readonly bool _isSharedContext;

        protected DatabaseTestBase(AppDbContext context = null)
        {
            if (context != null)
            {
                Context = context;
                _isSharedContext = true;
            }
            else
            {
                Context = TestDbContextFactory.CreateInMemoryContext();
                _isSharedContext = false;
            }
        }

        protected virtual void SeedTestData()
        {
        }

        protected virtual void ClearTestData()
        {
            if (!_isSharedContext)
            {
                Context.OrderItems.RemoveRange(Context.OrderItems);
                Context.Orders.RemoveRange(Context.Orders);
                Context.CartItems.RemoveRange(Context.CartItems);
                Context.FavouriteBooks.RemoveRange(Context.FavouriteBooks);
                Context.BookReviews.RemoveRange(Context.BookReviews);
                Context.Books.RemoveRange(Context.Books);
                Context.Users.RemoveRange(Context.Users);
                Context.SaveChanges();
            }
        }

        public virtual void Dispose()
        {
            if (!_isSharedContext)
            {
                Context?.Dispose();
            }
        }
    }
}
