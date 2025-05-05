using App;

namespace SpaceBattle
{
    public class RegisterIoCDependencyCustomBehaviorObjectWrapper : ICommand
    {
        public void Execute()
        {
            Ioc.Resolve<ICommand>("IoC.Register", "CustomBehaviorWrapper", (object[] args) => {
                var obj = (IDictionary<string, object>)args[0];
                var customBehavior = (IDictionary<string, Func<object>>) args[1];
                return new DictionaryWrapper(obj, customBehavior);
            }).Execute();
        }
    }
}