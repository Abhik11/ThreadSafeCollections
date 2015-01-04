#include "stdafx.h"
#include <iostream>
#include <Windows.h>
#include <assert.h>
#include <string>
#include <process.h>
using namespace std;

template<typename T>
struct Node
{
	T value;
	Node<T>* next;
};

template<typename T>
class ConcurrentStack
{
	volatile Node<T>* stackTop;

	/* Disallow copy-construction */
	ConcurrentStack(const ConcurrentStack<T>&);

	/* Disallow assignment */
	const ConcurrentStack<T>& operator= (const ConcurrentStack<T>&);
public:
	ConcurrentStack() : stackTop(NULL) {}

	/* Adds a new value to the stack top.
	* Guarantees that the top of the stack hold this new value. */
	void push(_In_ const T&);

	/* Gets the top element of the stack at a certain instant of time.
	* 'Returns' NULL if the stack does not contain any elements.
	* Note that there is no guarantee that the value is correct after the function 'returns'
	* since another thread could have mutated the stack. */
	void top(_Out_ T&) const;

	/* Deletes the top element of the stack. */
	void pop();

	/* Gets the top element of the stack and deletes it.
	* Gurarantees that the element deleted is the same whose value is 'returned'.
	* 'Returns' NULL if the stack is empty.
	* It is recommended to call this function instead of calling top() followed by pop(). */
	void getTopAndPop(_Out_ T&);

	/* Returns true if the stack does not contain any element.
	* Note that there is no guarantee that the result is valid after the function returns.
	* There is, however, a guarantee that if no thread mutates the state of the stack,
	* a subsequent call to isEmpty() will also return true. */
	bool isEmpty()
	{
		return NULL == stackTop;
	}
	~ConcurrentStack();
};