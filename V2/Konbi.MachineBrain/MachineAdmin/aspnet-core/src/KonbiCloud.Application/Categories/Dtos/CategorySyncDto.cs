using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Categories.Dtos
{
    class CategorySyncDto
    {
    }
}
public class CategorySyncDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; }
}