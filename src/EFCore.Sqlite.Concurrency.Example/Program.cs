

using EFCore.Sqlite.Concurrency.Example;


using (var db = new BloggingContext())
{
    var blog = db.Blogs.Find(1);
    if (blog is null)
    {
        db.Blogs.Add(new Blog{
            BlogId = 1,
            Url = "blog.com"
        });

        db.SaveChanges();

        return;
    }

    // Simulate a concurrent update
    using (var concurrentDb = new BloggingContext())
    {
        var currentBlog = concurrentDb.Blogs.Find(1);
        currentBlog!.Url = "newUrl";
        concurrentDb.SaveChanges();
    }

    // Throws DbUpdateConcurrencyException
    blog!.Url = "ConcUrl";
    db.SaveChanges();
}

