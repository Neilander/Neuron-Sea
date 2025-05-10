using System;
using System.Linq.Expressions;
using UnityEngine;

public static class HelperToolkit
{
    /// <summary>
    /// 打印出每个 bool 表达式的名称和当前值。
    /// 用法：HelperToolkit.PrintBoolStates(() => isRunning, () => hasHealth, ...);
    /// </summary>
    public static void PrintBoolStates(params Expression<Func<bool>>[] boolExpressions)
    {
        System.Text.StringBuilder sb = new();

        foreach (var expr in boolExpressions)
        {
            string name = GetExpressionName(expr.Body);
            bool value;

            try
            {
                value = expr.Compile().Invoke();
            }
            catch (Exception ex)
            {
                sb.AppendLine($"{name}: [Error - {ex.Message}]");
                continue;
            }

            sb.AppendLine($"{name}: {value}");
        }

        Debug.Log(sb.ToString());
    }

    private static string GetExpressionName(Expression expr)
    {
        return expr switch
        {
            MemberExpression memberExpr => memberExpr.Member.Name,
            UnaryExpression unaryExpr when unaryExpr.Operand is MemberExpression member => member.Member.Name,
            _ => expr.ToString()
        };
    }
}
