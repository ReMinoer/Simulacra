using System.Linq.Expressions;

namespace Simulacra.Binding.Utils
{
    static public class ExpressionUtils
    {
        static public MemberExpression GetPropertyMemberExpression(LambdaExpression modelPropertyGetterExpression)
        {
            Expression body = modelPropertyGetterExpression.Body;
            if (body is UnaryExpression unaryExpression)
                body = unaryExpression.Operand;

            return (MemberExpression)body;
        }
    }
}