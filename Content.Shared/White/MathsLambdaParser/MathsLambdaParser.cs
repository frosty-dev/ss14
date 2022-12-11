using System.Linq;

namespace Content.Shared.White.LambdaParser;

//скриптинг и деревья выражений заборонили, придеца костылять кокэндболлинг..........
public static class MathsLambdaParser
{
    public static Func<float, float>? ToFloatLambda(string? opsStr)
    {
        if (string.IsNullOrWhiteSpace(opsStr))
            return null;

        Func<float, float>? res = null;
        foreach (var item in opsStr.Split(';'))
        {
            var op = ParseOp(item);
            if (op == null)
                continue;
            if (res == null)
                res = (x) => op(x);
            else
            {
                var prev = res;
                res = (x) => op(prev(x));
            }
        }
        return res;
    }

    private static Func<float, float>? ParseOp(string opStr)
    {
        try
        {
            var args = opStr.Trim().Split(' ');
            var argsNums = args.Select(x => float.TryParse(x, out var res) ? res : 0f).ToArray();
            var op = Enum.Parse<ToFloatLambdaOp>(args[0]);

            return op switch
            {
                ToFloatLambdaOp.Add => (x) => x + argsNums[1],
                ToFloatLambdaOp.Sub => (x) => x - argsNums[1],
                ToFloatLambdaOp.Mul => (x) => x * argsNums[1],
                ToFloatLambdaOp.Div => (x) => x / argsNums[1],
                ToFloatLambdaOp.Mod => (x) => x % argsNums[1],
                ToFloatLambdaOp.Clamp => (x) => Math.Clamp(x, argsNums[1], argsNums[2]),
                _ => throw new ArgumentException("sussi imposta")
            };
        }
        catch
        {
            return null;
        }
    }
}

internal enum ToFloatLambdaOp
{
    Add,
    Sub,
    Mul,
    Div,
    Mod,
    Clamp
}
