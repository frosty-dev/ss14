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
            var op = Enum.Parse<ToFloatLambdaOp>(args[0]);

            return op switch
            {
                ToFloatLambdaOp.Add => float.TryParse(args[1], out var op1) ? (x) => x + op1 : null,
                ToFloatLambdaOp.Sub => float.TryParse(args[1], out var op1) ? (x) => x - op1 : null,
                ToFloatLambdaOp.Mul => float.TryParse(args[1], out var op1) ? (x) => x * op1 : null,
                ToFloatLambdaOp.Div => float.TryParse(args[1], out var op1) ? (x) => x / op1 : null,
                ToFloatLambdaOp.Mod => float.TryParse(args[1], out var op1) ? (x) => x % op1 : null,
                ToFloatLambdaOp.Clamp => float.TryParse(args[1], out var op1) && float.TryParse(args[2], out var op2)
                    ? (x) => Math.Clamp(x, op1, op2) : null,
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
