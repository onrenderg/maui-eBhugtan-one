// JavaScript (what you just ran):
let departments = [
    {deptId:"3", name:"Finance", isChecked:true},
    {deptId:"5", name:"Personnel", isChecked:false},
    {deptId:"12", name:"Education", isChecked:true}
];
let selectedDepts = departments
    .filter(function(d){ return d.isChecked })
    .map(function(d){ return d.deptId });
console.log(selectedDepts);  // ["3","12"]



// C# equivalent (in your app):

public class Department
{
    [PrimaryKey]
    public string? DeptId { get; set; }
    public string? DeptName { get; set; }
    public bool IsChecked { get; set; }
}
List<Department> Departments = new List<Department>
{
    new Department { DeptId="3",  DeptName="Finance",   IsChecked=true  },
    new Department { DeptId="5",  DeptName="Personnel", IsChecked=false },
    new Department { DeptId="12", DeptName="Education", IsChecked=true  }
};
var selectedDepts = Departments
    .Where(d => d.IsChecked)       // .filter()
    .Select(d => d.DeptId)         // .map()
    .ToList();                     // makes it a List
// selectedDepts = ["3", "12"]
