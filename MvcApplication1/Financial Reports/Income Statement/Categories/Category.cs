using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExcoUtility;

namespace MvcApplication1.Financial_Reports.Income_Statement.Categories
{
    public class Category
    {
        public List<Group> groupList = new List<Group>();
        public string name = string.Empty;

        public Category(string name)
        {
            this.name = name;
        }
    }
}