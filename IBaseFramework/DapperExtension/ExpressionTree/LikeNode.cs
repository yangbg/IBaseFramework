namespace IBaseFramework.DapperExtension.ExpressionTree
{
    internal class LikeNode : Node
    {
        public LikeMethod Method { get; set; }
        public MemberNode MemberNode { get; set; }
        public object Value { get; set; }
    }
}
