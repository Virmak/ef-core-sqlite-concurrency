
using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace ConcurLite.Example;

public partial class Progran
{
    public static void Main(string[] argv)
    {
        Console.WriteLine("Main hello ");


Assembly assembly = Assembly.GetExecutingAssembly();

        // Retrieve custom attributes applied to the assembly
        object[] attributes = assembly.GetCustomAttributes(true);

        // Iterate through each attribute and display its properties
        foreach (object attribute in attributes)
        {
            // Check if the attribute is of the type you're interested in
            if (attribute is AssemblyTitleAttribute)
            {
                AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attribute;
                Console.WriteLine("Title: " + titleAttribute.Title);
            }
            else if (attribute is AssemblyDescriptionAttribute)
            {
                AssemblyDescriptionAttribute descriptionAttribute = (AssemblyDescriptionAttribute)attribute;
                Console.WriteLine("Description: " + descriptionAttribute.Description);
            }
            // Add more conditions for other attributes if needed
        }

        HelloFrom("source");
    }

    static partial void HelloFrom(string name);
}

// new BloggingContext();


// using (var db = new BloggingContext())
// {
//     var blog = db.Blogs.Find(1);

//     // Simulate a concurrent update
//     using (var concurrentDb = new BloggingContext())
//     {
//         var currentBlog = concurrentDb.Blogs.Find(1);
//         currentBlog!.Url = "newUrl";
//         concurrentDb.SaveChanges();
//     }

//     // Throws DbUpdateConcurrencyException
//     blog!.Url = "ConcUrl";
//     db.SaveChanges();
// }

