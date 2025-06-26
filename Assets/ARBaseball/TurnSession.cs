using System;
using UnityEngine;

public class TurnSession : MonoBehaviour
{
    public int ballCount = 0;
    public int strikeCount = 0;
    public int outCount = 0;

    public Action<TurnResult, int> OnTurnResultAccepted;
    public Action<TurnSessionResult, int> NoticeTurnSessionResult;
    public void AcceptTurnResult(TurnResult turnResult)
    {
        switch(turnResult)
        {
            case TurnResult.Ball:
            {
                ballCount++;
                
                if (ballCount >= 4)
                {
                    // 게임 결과 처리
                    NoticeTurnSessionResult?.Invoke(TurnSessionResult.Complete, 0);
                    ResetTurnSession();
                }

                OnTurnResultAccepted?.Invoke(turnResult, ballCount);

                break; 
            }
            case TurnResult.Strike:
                {
                    strikeCount++;

                    if (strikeCount >= 3)
                    {
                        // 아웃처리
                        outCount++;
                        NoticeTurnSessionResult?.Invoke(TurnSessionResult.Out, outCount);
                        if (outCount >= 3)
                        {
                            NoticeTurnSessionResult?.Invoke(TurnSessionResult.SessionEnd, 0);
                            ResetTurnSession();
                        }
                        ResetTurn();
                    }

                    OnTurnResultAccepted?.Invoke(turnResult, strikeCount);

                    break;
                }
            case TurnResult.Foul:
                {
                    if (strikeCount >= 2)
                    {
                        break;
                    }
                    strikeCount++;
                    OnTurnResultAccepted?.Invoke(turnResult, strikeCount);
                    break;
                }
            case TurnResult.Hit:
            case TurnResult.HomeRun:
                NoticeTurnSessionResult?.Invoke(TurnSessionResult.Complete, 0);
                ResetTurnSession();
                break; 
        }
    }

    public void ResetTurn()
    {
        strikeCount = 0;
        ballCount = 0;
    }

    public void ResetTurnSession()
    {
        ResetTurn();
        outCount = 0;
    }
}