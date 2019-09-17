using System;
using System.Threading.Tasks;
using Microsoft.DotNet.Interactive;
using System.Reactive.Linq;
using static Microsoft.DotNet.Interactive.Kernel;
using Microsoft.DotNet.Interactive.Commands;
using System.Reactive;
using System.Reactive.Concurrency;
using Microsoft.DotNet.Interactive.Rendering;

namespace SampleExtension
{
    public class SampleExtension : IKernelExtension
    {
        // private static void Main(string[] args)
        // {
        //     IObservable<string> obj = Observable.Generate(
        //         0, //Sets the initial value like for loop    
        //         _ => true, //Don't stop till i say so, infinite loop    
        //         i => i + 1, //Increment the counter by 1 everytime    
        //         i => new string('#', i), //Append #    
        //         i => TimeSelector(i)); //delegated this to private method which just calculates time    

        //     //Subscribe here    
        //     using (obj.Materialize().Subscribe(Console.WriteLine))
        //     {
        //         Console.WriteLine("Press any key to exit!!!");
        //         Console.ReadLine();
        //     }
        // }

        public static void trace(IObservable<string> observable, string mimeType = HtmlFormatter.MimeType)
        {
            var kernel = KernelInvocationContext.Current.HandlingKernel;
            Task.Run(() => kernel.SendAsync(new SubmitCode(@"var d = display(""starting"");")));
            var source = Observable.Interval(TimeSpan.FromSeconds(1), CurrentThreadScheduler.Instance);

            var displayId = Guid.NewGuid().ToString();

            Task.Run(() =>
            {
                var value = "displaying observable";
                var formatted = new FormattedValue(
                mimeType,
                value.ToDisplayString(mimeType));

                DisplayValue d = new DisplayValue(value, formatted, displayId);
                return kernel.SendAsync(d);
            }).Wait();

            observable.Materialize().Take(10).Subscribe(e =>
            {
                var formatted = new FormattedValue(
                mimeType,
                e.ToDisplayString(mimeType));

                Task.Run(() => kernel.SendAsync(new UpdateDisplayedValue(e, formatted, displayId)));
            });
        }

        //Returns TimeSelector    
        private static TimeSpan TimeSelector(int i)
        {
            return TimeSpan.FromSeconds(i);
        }

        public async Task OnLoadAsync(IKernel kernel)
        {
            await kernel.SendAsync(new SubmitCode($"1+2"));
            await kernel.SendAsync(new SubmitCode($"using static {typeof(SampleExtension).FullName};"));
        }
    }
}
