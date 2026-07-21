using UnityEngine;

public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
{
    //씬 전환 시 삭제할지 여부
    protected bool m_IsDestroyOnLoad = false;

    //이 클래스의 스태틱 인스턴스 변수
    protected static T m_Instance;

    //얘가 객체 받는 부분
    public static T Instance
    {
        get { return m_Instance; }
    }

    private void Awake()
    {
        Init();
    }

    //Awake가 아닌 Init에서 초기화 작업. 만약 초기화가 필요하면 이걸 override해서 사용
    protected virtual void Init()
    {
        if(m_Instance == null)
        {
            m_Instance = (T)this;

            if(!m_IsDestroyOnLoad)
            {
                DontDestroyOnLoad(this);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    //삭제 시 실행되는 함수
    protected virtual void OnDestroy()
    {
        Dispose();
    }

    //삭제 시 추가로 처리해 주어야할 작업을 여기서 처리
    protected virtual void Dispose()
    {
        m_Instance = null;
    }
}
