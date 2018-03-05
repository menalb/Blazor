using System.Collections.Generic;

namespace BasicTestApp.Groups
{
    public class GroupsViewModel
    {
        public IEnumerable<Group> Groups { get; set; }
        public int TotalGroupsCount { get; set; }
    }

    public class Group
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }
}