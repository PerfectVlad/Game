using App;
using App.Scopes;

namespace SpaceBattle.Tests
{
    public class RegisterIoCDependencyCustomBehaviorWrapper
    {
        [Fact]
        public void Execute_ShouldRegisterSatCollisionCheckerDependency()
        {
            // Arrange
            new InitCommand().Execute();
            var iocScope = Ioc.Resolve<object>("IoC.Scope.Create");
            Ioc.Resolve<ICommand>("IoC.Scope.Current.Set", iocScope).Execute();

            var obj = new Dictionary<string, object>();
            var customBehavior = new Dictionary<string, Func<object>>();

            var registrator = new RegisterIoCDependencyCustomBehaviorObjectWrapper();

            // Act
            registrator.Execute();

            // Assert
            var checker = Ioc.Resolve<IDictionary<string, object>>("CustomBehaviorWrapper", obj, customBehavior);

            Assert.IsAssignableFrom<IDictionary<string, object>>(checker);
        }
    }
}