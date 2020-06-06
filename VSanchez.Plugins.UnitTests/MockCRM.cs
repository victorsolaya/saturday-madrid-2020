using FakeXrmEasy;
using VSanchez.Plugins.EntityMetadata;

namespace VSanchez.Plugins.UnitTests
{
    public class MockCRM
    {
        public XrmFakedContext xrmContext { get; set; }
        public XrmFakedPluginExecutionContext xrmContextPlugin { get; set; }


        public MockCRM()
        {
            InitialiseFakeXrm();
        }

        private void InitialiseFakeXrm()
        {
            xrmContextPlugin = new XrmFakedPluginExecutionContext();
            xrmContext = new XrmFakedContext();
            var assemblyEntities = System.Reflection.Assembly.GetAssembly(typeof(Account));
            xrmContext.InitializeMetadata(assemblyEntities);
            xrmContext.ProxyTypesAssembly = assemblyEntities;
        }
    }
}
