using App;

namespace SpaceBattle
{
    public class RegisterIoCDepenedncyAdapterBuilder : ICommand
    {
        public void Execute()
        {
            Ioc.Resolve<ICommand>("IoC.Register", "Adapter.Instance", (object[] args) => {
                var interfaceType = (Type) args[0];
                var obj = (IDictionary<string, object>)args[1];
                var customBehavior = (IDictionary<string, Func<object>>)args[2];

                var adapterCode = Ioc.Resolve<string>("Adapters.CodeGenerator", interfaceType);
                var adapterType = Ioc.Resolve<Type>("Adapter.Compile", adapterCode);
                var objWithCustomBehavior = Ioc.Resolve<IDictionary<string, object>>("CustomBehaviorWrapper", obj, customBehavior);

                var adapter = Activator.CreateInstance(adapterType, objWithCustomBehavior);

                return adapter;
            }).Execute();
        }
    }
}