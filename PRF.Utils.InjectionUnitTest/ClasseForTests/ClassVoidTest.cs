using System;
using System.Threading;
using System.Threading.Tasks;
using PRF.Utils.Injection.Utils;

namespace PRF.Utils.InjectionUnitTest.ClasseForTests
{
    // ReSharper disable UnusedParameter.Global
    public interface IClassVoidTest
    {
        void MethodCall();
        int Prop { get; set; }
        void MethodCallWithParameters(string param1, int param2, object param3);
        double MethodCallWithParametersWithReturn(string param1, int param2, object param3);
        bool TryGetValue(bool test, out double returnValue);
        double MethodCallWithParametersWithParams(params string[] parameters);
        Task MethodCallWaitAsync(TimeSpan wait);
        Task<int> MethodCallWaitWithReturnAsync(TimeSpan wait);
        Task<CustomObject> MethodCallWaitWithReturnDataAsync(TimeSpan wait);
        Task<CustomObject> MethodCallForPerf();
        void MethodCallWait(TimeSpan wait);
        Task<int> MethodCallForPerfInt();
    }
    // ReSharper restore UnusedParameter.Global

    public class CustomObject
    {
        public override string ToString()
        {
            return "surchargeCustomObject";
        }
    }

    public sealed class ClassVoidTest : IClassVoidTest
    {
        public void MethodCall()
        {
            // juste un appel vide
        }

        public int Prop { get; set; }

        public void MethodCallWithParameters(string param1, int param2, object param3)
        {
            // juste un appel vide
        }

        public double MethodCallWithParametersWithReturn(string param1, int param2, object param3)
        {
            // juste un appel vide
            return 569.489;
        }

        public bool TryGetValue(bool test, out double returnValue)
        {
            returnValue = 987.6;
            return true;
        }

        public double MethodCallWithParametersWithParams(params string[] parameters)
        {
            // juste un appel vide
            return -1;
        }

        public async Task MethodCallWaitAsync(TimeSpan wait)
        {
            await Task.Delay(wait).ConfigureAwait(false);
        }

        public async Task<int> MethodCallWaitWithReturnAsync(TimeSpan wait)
        {
            await Task.Delay(wait).ConfigureAwait(false);
            return 42;
        }

        public async Task<CustomObject> MethodCallWaitWithReturnDataAsync(TimeSpan wait)
        {
            await Task.Delay(wait).ConfigureAwait(false);
            return new CustomObject();
        }

        public async Task<CustomObject> MethodCallForPerf()
        {
            return await Task.FromResult(new CustomObject());
        }

        public async Task<int> MethodCallForPerfInt()
        {
            return await Task.FromResult(42);
        }

        public void MethodCallWait(TimeSpan wait)
        {
            Thread.Sleep(wait);
        }
    }

    public interface IClassVoidAttributeTest
    {
        [Interception]
        void MethodCall();

        // pour l'interception par attribut, il faut mettre les attributs sur le get ET sur le set indépendamment
        int Prop
        {
            [Interception]
            get;

            [Interception]
            set;
        }
    }

    public sealed class ClassVoidAttributeTest : IClassVoidAttributeTest
    {
        public void MethodCall()
        {
            // juste un appel vide
        }

        public int Prop { get; set; }
    }
    
}