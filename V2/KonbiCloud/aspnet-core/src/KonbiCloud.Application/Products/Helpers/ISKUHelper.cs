using Abp.Dependency;
using Abp.Domain.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Products.Helpers
{
    public interface ISKUHelper: ITransientDependency
    {
        string Generate(int length = 16);
        bool Validate(string input);
    }
}
