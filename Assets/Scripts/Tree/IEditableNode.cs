using Gen.Data;

namespace Gen.Tree
{
    public interface IEditableNode
    {
        public Person Person { get; set; }
        public void OnPersonDeletion();
    }
}
