namespace Protoinject
{
    public interface IBindUnique
    {
        void EnforceOnePerScope();
        void AllowManyPerScope();
    }
}