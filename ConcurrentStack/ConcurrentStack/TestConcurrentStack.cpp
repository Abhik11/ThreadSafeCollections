#include "ConcurrentStack.cpp"
unsigned __stdcall ThreadStaticEntryPoint(void * pThis)
{
	ConcurrentStack<int>* s = (ConcurrentStack<int>*)pThis;
	//s->push(1);
	//s->push(2);
	//s->push(3);
	//s->push(4);
	s->pop();
	return 1;
}
int _tmain(int argc, _TCHAR* argv [])
{
	ConcurrentStack<int>* currStack = new ConcurrentStack < int >;
	unsigned  uiThread1ID;
	currStack->push(3);
	currStack->push(1);
	HANDLE hth1 = (HANDLE) _beginthreadex(
		NULL,
		0,
		ThreadStaticEntryPoint,
		currStack,           // arg list
		CREATE_SUSPENDED,  // so we can later call ResumeThread()
		&uiThread1ID);
	ResumeThread(hth1);
	Sleep(10);
	currStack->pop();
	currStack->push(2);
	currStack->push(1);
	WaitForSingleObject(hth1, INFINITE);
	CloseHandle(hth1);
	delete currStack;
	currStack = NULL;

	return 0;
}

class BeforeAndAfterMain
{
public:
	BeforeAndAfterMain()
	{
		cout << "Before Main\n\n";
	}
	~BeforeAndAfterMain()
	{
		cout << "\n\nAfter Main\n";
		system("pause");
	}
};
BeforeAndAfterMain m;