using System.ComponentModel;

namespace Protoinject
{
    public interface ICurrentNode
    {
        void SetName(string name);

        /// <summary>
        /// Do not use this method.
        /// </summary>
        /// <returns>The raw node reference for implementing factories.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        INode GetNodeForFactoryImplementation();
    }
}