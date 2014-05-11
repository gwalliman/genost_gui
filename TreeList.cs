using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CapGUI
{
    public class TreeList<T>
    {
        protected class Node
        {
            public Node Next;
            public Node Child;
            public int Index;
            public T Data;
        }

        protected Node head = null;
        protected Node previousNode = null;
        protected Node addAfterBranchNode = null; //used for special add case
        protected bool atIndex = false; //found index used for special add case
        protected ObservableCollection<T> returnList; //for blocks

        //works
        protected Node FindNode(Node current, int index)
        {
            atIndex = false;
            if (current.Index == index)
            {
                atIndex = true;
                return current;
            }
            else if (current.Next != null && current.Next.Index <= index)
            {
                previousNode = current;
                return FindNode(current.Next, index);
            }
            else if (current.Child != null && current.Child.Index <= index)
            {
                previousNode = current;
                addAfterBranchNode = current;
                return FindNode(current.Child, index);
            }
            else
            {
                previousNode = current;
                return current;
            }
        }

        #region Add Node
        //works
        protected void AddToHead(T t)
        {
            Node newNode = new Node();
            newNode.Next = head;
            newNode.Child = null;
            newNode.Index = 0;
            newNode.Data = t;
            head = newNode;

        }

        //works
        protected Node AddNode(Node current, int index, T t)
        {
            Node newNode = new Node();
            newNode.Next = current;
            newNode.Child = null;
            newNode.Index = index;
            newNode.Data = t;
            previousNode.Next = newNode;
            return newNode;
        }

        //works
        protected Node AddNodeAfterBranch(Node current, int index, T t)
        {
            Node newNode = new Node();
            newNode.Next = null;
            newNode.Child = null;
            newNode.Index = index;
            newNode.Data = t;
            current.Next = newNode;
            return newNode;
        }

        //works
        protected Node AddNodeToChildBranch(Node current, int index, T t)
        {
            Node newNode = new Node();
            newNode.Next = current;
            newNode.Child = null;
            newNode.Index = index;
            newNode.Data = t;
            previousNode.Child = newNode;
            return newNode;
        }

        //works
        protected void AddChild(Node current, T t)
        {
            Node newNode = new Node();
            newNode.Next = null;
            newNode.Child = null;
            newNode.Data = t;
            current.Child = newNode;
        }

        //works
        public virtual ObservableCollection<T> AddWithChild(T nodeData, T childData, int index)
        {
            if (index == 0)
            {
                AddToHead(nodeData);
                AddChild(head, childData);
            }
            else
            {
                Node currentNode = FindNode(head, index);
                if (addAfterBranchNode != null && addAfterBranchNode.Next == null && !atIndex)
                    currentNode = AddNodeAfterBranch(addAfterBranchNode, index, nodeData);
                else if (currentNode == previousNode)
                    currentNode = AddNode(null, index, nodeData);
                else if (previousNode.Child == currentNode)
                    currentNode = AddNodeToChildBranch(currentNode, index, nodeData);
                else
                    currentNode = AddNode(currentNode, index, nodeData);
                AddChild(currentNode, childData);
            }
            returnList = new ObservableCollection<T>();
            ResetIndex(head, 0);
            return returnList;
        }

        //works
        public virtual ObservableCollection<T> Add(T nodeData, int index)
        {
            
            if (index == 0)
                AddToHead(nodeData);
            else
            {
                Node currentNode = FindNode(head, index);
                if (addAfterBranchNode != null && addAfterBranchNode.Next == null && !atIndex)
                    AddNodeAfterBranch(addAfterBranchNode, index, nodeData);
                else if (currentNode == previousNode)
                    AddNode(null, index, nodeData);
                else if (previousNode.Child == currentNode)
                    AddNodeToChildBranch(currentNode, index, nodeData); 
                else
                    AddNode(currentNode, index, nodeData);
            }
            returnList = new ObservableCollection<T>();
            ResetIndex(head, 0);
            return returnList;
        }

        #region Non List Returning Adds
        //being used to add with the load funciton that doesn't create and transfer list elements
        public virtual void AddWithChildNoReturn(T nodeData, T childData, int index)
        {
            if (index == 0)
            {
                AddToHead(nodeData);
                AddChild(head, childData);
            }
            else
            {
                Node currentNode = FindNode(head, index);
                if (addAfterBranchNode != null && addAfterBranchNode.Next == null && !atIndex)
                    currentNode = AddNodeAfterBranch(addAfterBranchNode, index, nodeData);
                else if (currentNode == previousNode)
                    currentNode = AddNode(null, index, nodeData);
                else if (previousNode.Child == currentNode)
                    currentNode = AddNodeToChildBranch(currentNode, index, nodeData);
                else
                    currentNode = AddNode(currentNode, index, nodeData);
                AddChild(currentNode, childData);
            }
            ResetIndex(head, 0);
        }

        //being used to add with the load funciton that doesn't create and transfer list elements
        public virtual void AddNoReturn(T nodeData, int index)
        {

            if (index == 0)
                AddToHead(nodeData);
            else
            {
                Node currentNode = FindNode(head, index);
                if (addAfterBranchNode != null && addAfterBranchNode.Next == null && !atIndex)
                    AddNodeAfterBranch(addAfterBranchNode, index, nodeData);
                else if (currentNode == previousNode)
                    AddNode(null, index, nodeData);
                else if (previousNode.Child == currentNode)
                    AddNodeToChildBranch(currentNode, index, nodeData);
                else
                    AddNode(currentNode, index, nodeData);
            }
            ResetIndex(head, 0);
        }
        #endregion

        #endregion

        #region Delete node
        //not tested
        protected void DeleteHeadNode(Node current)
        {
            head = current.Next;
            current.Next = null;
            RemoveRefrences(current);
        }

        //not tested
        protected void DeleteNode(Node current)
        {
            previousNode.Next = current.Next;
            current.Next = null;
            RemoveRefrences(current);
        }

        //not tested
        protected void DeleteChild(Node current)
        {
            previousNode.Child = current.Next;
            current.Next = null;
            RemoveRefrences(current);
        }

        //not tested
        protected void RemoveRefrences(Node current)
        {
            if (current.Child != null)
            {
                RemoveRefrences(current.Child);
            }
            if (current.Next != null)
            {
                RemoveRefrences(current.Next);
            }
            current = null;
            GC.Collect();
        }

        //not tested
        public virtual ObservableCollection<T> Delete(int index)
        {
            if (index == 0)
                DeleteHeadNode(head);
            else
            {
                Node current = FindNode(head, index);
                if (previousNode.Child == current && current.Next != null)
                    DeleteChild(current);
                else
                    DeleteNode(current);

            }
            returnList = new ObservableCollection<T>();
            ResetIndex(head, 0);
            return returnList;
        }
        #endregion

        #region Move Node
        //works
        #region Move Functins
        protected void MoveFromHeadNode(Node toNode)
        {
            Node current = head;
            head = current.Next;
            if (current.Index < toNode.Index)
            {
                current.Next = toNode.Next;
                toNode.Next = current;
            }
            else
            {
                current.Next = toNode;
                previousNode.Next = current;
            }
        }

        //works
        protected void MoveHeadToChildNode(Node toNode)
        {         
            Node current = head;
            head = current.Next;            
            if (current.Index < toNode.Index)
            {
                current.Next = toNode.Child;
                toNode.Child = current;
            }
            else
            {
                current.Next = toNode;
                previousNode.Child = current;
            }
            
        }

        //works
        protected void MoveToHeadNode(Node fromNode)
        {
            previousNode.Next = fromNode.Next;
            fromNode.Next = head;
            head = fromNode;
        }

        //works
        protected void MoveChildToHeadNode(Node fromNode)
        {
            previousNode.Child = fromNode.Next;
            fromNode.Next = head;
            head = fromNode;
        }

        //works
        protected void MoveHeadToAfterChildBranch(Node toNode)
        {
            Node current = head;
            head = current.Next;
            current.Next = addAfterBranchNode.Next;
            addAfterBranchNode.Next = current;
        }

        //works
        protected void MoveFromChildNode(Node fromNode, Node fromPrevious, Node toNode)
        {
            fromPrevious.Child = fromNode.Next;
            if (fromNode.Index < toNode.Index)
            {
                fromNode.Next = toNode.Next;
                toNode.Next = fromNode;
            }
            else
            {
                fromNode.Next = toNode;
                previousNode.Next = fromNode;
            }
        }

        //works
        protected void MoveToChildNode(Node fromNode, Node fromPrevious, Node toNode)
        {
            fromPrevious.Next = fromNode.Next;           
            if (fromNode.Index < toNode.Index)
            {
                fromNode.Next = toNode.Child;
                toNode.Child = fromNode;
            }
            else
            {
                fromNode.Next = toNode;
                previousNode.Child = fromNode;
            }
        }

        //works
        protected void MoveFromChildToChildNode(Node fromNode, Node fromPrevious, Node toNode)
        {
            fromPrevious.Child = fromNode.Next;           
            if (fromNode.Index < toNode.Index)
            {
                fromNode.Next = toNode.Child;
                toNode.Child = fromNode;
            }
            else
            {
                fromNode.Next = toNode;
                previousNode.Child = fromNode;
            }
        }

        //works 
        protected void MoveNode(Node fromNode, Node fromPrevious, Node toNode)
        {
            fromPrevious.Next = fromNode.Next;
            if (fromNode.Index < toNode.Index)
            {
                fromNode.Next = toNode.Next;
                toNode.Next = fromNode;
            }
            else
            {
                fromNode.Next = toNode;
                previousNode.Next = fromNode;
            }

        }

        //works
        protected void MoveToAfterChildBranch(Node fromNode, Node fromPrevious)
        {
            if (fromPrevious.Child == fromNode)
            {
                fromPrevious.Child = fromNode.Next;
                fromNode.Next = addAfterBranchNode.Next;
                addAfterBranchNode.Next = fromNode;
            }
            else
            {
                fromPrevious.Next = fromNode.Next;
                fromNode.Next = addAfterBranchNode.Next;
                addAfterBranchNode.Next = fromNode;
            }
        }

        #endregion

        public virtual ObservableCollection<T> Move(int fromIndex, int toIndex)
        {
            if (fromIndex != toIndex)
            {
                if (fromIndex == 0)
                {
                    Node current = FindNode(head, toIndex);

                    FromHeadMove(fromIndex, toIndex, current);
                }
                else if (toIndex == 0)
                {
                    Node current = FindNode(head, fromIndex);

                    ToHeadMove(fromIndex, toIndex, current);
                }
                else
                {
                    Node fromNode = FindNode(head, fromIndex);
                    Node fromPrevious = previousNode;
                    Node toNode = FindNode(head, toIndex);
       
                    GeneralMove(fromIndex, toIndex, fromNode, fromPrevious, toNode);
                }
            }
            //addAfterBranchNode = null;
            returnList = new ObservableCollection<T>();
            ResetIndex(head, 0);
            return returnList;
        }

        protected void ToHeadMove(int fromIndex, int toIndex, Node current)
        {

            if (previousNode.Child == current && current.Next != null)
                MoveChildToHeadNode(current);
            else
                MoveToHeadNode(current);
        }

        protected void FromHeadMove(int fromIndex, int toIndex, Node current)
        {
           

            if (fromIndex < toIndex && head.Next != null && head.Child != null && head.Next.Index > toIndex) { }
            else if (fromIndex < toIndex && head.Next == null && head.Child != null) { }
            else if (fromIndex < toIndex && current.Next == null && addAfterBranchNode != null && (addAfterBranchNode.Next == null || addAfterBranchNode.Next.Index == (toIndex + 1)))
                MoveHeadToAfterChildBranch(current);
            else if (fromIndex < toIndex && current.Child != null)
                MoveHeadToChildNode(current);
            else if (previousNode == current)
                MoveFromHeadNode(null);
            else
                MoveFromHeadNode(current);
        }

        protected void GeneralMove(int fromIndex, int toIndex, Node fromNode, Node fromPrevious, Node toNode)
        {

            if (fromIndex < toIndex && fromNode.Next != null && fromNode.Child != null && fromNode.Next.Index > toIndex) { }
            else if (fromIndex < toIndex && fromNode.Next == null && fromNode.Child != null) { }
            else if (fromIndex < toIndex && toNode.Next == null && addAfterBranchNode != null && (addAfterBranchNode.Next == null || addAfterBranchNode.Next.Index == (toIndex + 1)))
                MoveToAfterChildBranch(fromNode, fromPrevious);
            else if ((fromIndex < toIndex && (fromPrevious.Child == fromNode) && (toNode.Child != null)) || ((fromPrevious.Child == fromNode) && (previousNode.Child == toNode)))
                MoveFromChildToChildNode(fromNode, fromPrevious, toNode);
            else if (fromPrevious.Child == fromNode && fromNode.Next != null)
            {
                if (previousNode == toNode)
                    MoveFromChildNode(fromNode, fromPrevious, null);
                else
                    MoveFromChildNode(fromNode, fromPrevious, toNode);
            }
            else if ((fromIndex < toIndex && toNode.Child != null) || previousNode.Child == toNode)
                MoveToChildNode(fromNode, fromPrevious, toNode);
            else
            {
                if (previousNode == toNode)
                    MoveNode(fromNode, fromPrevious, null);
                else
                    MoveNode(fromNode, fromPrevious, toNode);
            }
        }

        #endregion
        //works
        public void Print()
        {
            PrintTreeList(head);
        }

        //works
        protected void PrintTreeList(Node current)
        {
            while (current != null)
            {
                Debug.WriteLine(current.Data + " index: " + current.Index);
                if (current.Child != null)
                    PrintTreeList(current.Child);
                current = current.Next;
            }
        }

        //works
        protected virtual int ResetIndex(Node current, int index)
        {
            while (current != null)
            {
                returnList.Add(current.Data);
                current.Index = index;
                index++;
                if (current.Child != null)
                {
                    index = ResetIndex(current.Child, index);
                }
                current = current.Next;
            }
            return index;
        }

        //being used for the load
        public ObservableCollection<T> ReturnList()
        {
            returnList = new ObservableCollection<T>();
            ResetIndex(head, 0);
            return returnList;
        }
    }
}