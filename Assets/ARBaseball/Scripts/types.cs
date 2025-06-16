/// <summary>
/// 게임 상태를 정의하는 열거형입니다.
/// None, Initializing, Ready, Play, End
/// </summary>
public enum GameState
{
    None,           // 비정상적인 상태
    Initializing,   // 게임이 시작되지 않은 상태, 초기화가 필요한 상태
    Ready,          // 플레이어가 게임 플레이를 위해 모든 준비 상태가 완료된 상태
    Play,           // 게임이 실제로 진행 중인 상태
    End             // 게임이 종료된 상태, 승패가 결정되었거나 플레이어가 게임을 종료한 상태
}

/// <summary>
/// 플레이 모드를 정의하는 열거형입니다.
/// None, PitcherMode, BatterMode
/// </summary>
public enum PlayMode
{
    None,
    PitcherMode,
    BatterMode
}

/// <summary>
/// 멀티 플레이 모드를 정의하는 열거형입니다.
/// SinglePlayMode, MultiPlayMode
/// </summary>
public enum GameMode
{
    SinglePlayMode,
    MultiPlayMode
}

/// <summary>
/// 게임 결과를 정의하는 열거형입니다.
/// None, PlayerWin, AIWin
/// </summary>
public enum GameResult
{
    None,
    PlayerWin,
    AIWin,
}

/// <summary>
/// 각 턴의 결과를 정의하는 열거형입니다.
/// None, Strike, Ball, Foul, Hit, HomeRun, Walk
/// </summary>
public enum TurnResult
{
    None,        // 턴 결과가 없는 상태
    Strike,      // 스트라이크
    Ball,        // 볼
    Foul,        // 파울
    Hit,         // 히트
    HomeRun,     // 홈런
    Walk         // 볼넷
}

/// <summary>
/// 한 사람이 타석에 서 있는 동안 
/// 행동의 결과를 정의하는 열거형입니다.
/// None, Ready, Ongoing, Out, Complete 
/// </summary>
public enum TurnSessionResult
{
    None,        // 비정상
    Ready,       // 타석에 진입했지만 아직 투구 시작 전
    Ongoing,     // 투구 중, 세션 진행 중
    Out,         // 삼진 등으로 타석 종료됨
    Complete     // 히트, 홈런, 볼넷 등으로 종료됨
}

/// <summary>
/// 타입 정의를 위한 클래스입니다.
/// </summary>
public class types
{
    // 이 클래스는 타입 정의를 위한 용도로 사용됩니다.
}