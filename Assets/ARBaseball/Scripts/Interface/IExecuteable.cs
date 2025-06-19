public interface IExecuteable
{
    /// <summary>
    /// 명령을 실행하는 메서드입니다.
    /// 상위 객체가 하위 객체에게 명령을 전달할 때 사용됩니다.
    /// </summary>

    public void Execute(string command);
}