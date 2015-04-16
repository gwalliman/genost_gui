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
    /**
     * This class is used to handle the structure of the editor code tree within the editor drag / drop target.
     */
    public class BlockTreeList : TreeList<Block>
    {
        
        protected int marginSize = 0; //Margin Size is used to track the indentation for nesting blocks
        
        //Overrides TreeList's Move function
        public override System.Collections.ObjectModel.ObservableCollection<Block> Move(int fromIndex, int toIndex)
        {
            if (fromIndex != toIndex)
            {
                //If we're moving a block from the top, reset the head
                if (fromIndex == 0)
                {
                    //Get the current node
                    Node current = FindNode(head, toIndex);

                    //If this block is an IF statement, we need to treat it specially (since it can have Else If, Else, etc.)
                    if (isIF(head))
                    {
                        //Check what's going on
                        Node IfChain = CheckIfNodeMove(head, toIndex);
                        //If we're not doing something bad with an IF, we may proceed
                        if (IfChain != null)
                        {
                            //Move the whole IF chain away from the head
                            FromHeadMove(fromIndex, toIndex, current, IfChain);
                        }
                    }
                    //If it's not an IF statement, just move it
                    else
                    {
                        FromHeadMove(fromIndex, toIndex, current);
                    }
                }
                //Else, if we're moving TO the head
                else if (toIndex == 0)
                {
                    Node current = FindNode(head, fromIndex);

                    //If this block is an IF statement, we need to treat it specially (since it can have Else If, Else, etc.)
                    if (isIF(current))
                    {
                        //Check what's going on
                        Node IfChain = CheckIfNodeMove(current, fromIndex);
                        //If we're not doing something bad with an IF, we may proceed
                        if (IfChain != null)
                        {
                            //Move the whole IF chain to the head
                            ToHeadMove(fromIndex, toIndex, current, IfChain);
                        }
                    }
                    //If it's not an IF statement, just move it.
                    else
                    {
                        ToHeadMove(fromIndex, toIndex, current);
                    }
                }
                //Else, the head is not involved and we're just moving from one index to another
                else
                {
                    //Get the from node
                    Node fromNode = FindNode(head, fromIndex);
                    Node fromPrevious = previousNode;
                    //Get the to node
                    Node toNode = FindNode(head, toIndex);

                    //If we're moving an IF node
                    if (isIF(fromNode))
                    {
                        //Check what's going on
                        Node IfChain = CheckIfNodeMove(fromNode, toIndex);
                        //If we're not doing something bad with an IF, we may proceed
                        if (IfChain != null)
                        {
                            //Move the whole IF chain to the proper area
                            GeneralMove(fromIndex, toIndex, fromNode, fromPrevious, toNode, IfChain);
                        }
                    }
                    //If it's not an IF statement, just move it.
                    else
                    {
                        GeneralMove(fromIndex, toIndex, fromNode, fromPrevious, toNode);
                    }
                }
            }
            returnList = new ObservableCollection<Block>();

            //Reset the head
            ResetIndex(head, 0);
            return returnList;
            //return base.Move(fromIndex, toIndex);
        }

        //Moves an item to the HEAD. Handles the movement of an IF chain
        private void ToHeadMove(int fromIndex, int toIndex, Node current, Node IfChain)
        {

            //If we are moving an IF chain
            if (previousNode.Child == current && current.Next != null)
            {
                MoveChildToHeadNode(current, IfChain);
            }
            //Else, just move as normal
            else
            {
                MoveToHeadNode(current, IfChain);
            }
        }

        //Moving a block from the HEAD to a different place
        private void FromHeadMove(int fromIndex, int toIndex, Node current, Node IfChain)
        {
            //If we're trying to move the head above itself, or directly below itself
            //Do nothing
            if (fromIndex < toIndex && head.Next != null && head.Child != null && head.Next.Index > toIndex) 
            { 
            }
            //If we're not actually moving the head, i.e. it's the only block in the editor
            //Do nothing
            else if (fromIndex < toIndex && head.Next == null && head.Child != null) 
            { 
            }
            //Moving an IF chain
            else if (fromIndex < toIndex && current.Next == null && addAfterBranchNode != null && (addAfterBranchNode.Next == null || addAfterBranchNode.Next.Index == (toIndex + 1)))
            {
                MoveHeadToAfterChildBranch(current, IfChain);
            }
            //Moving an IF chain 
            else if (fromIndex < toIndex && current.Child != null)
            {
                MoveHeadToChildNode(current, IfChain);
            }
            //Moving head
            else if (previousNode == current)
            {
                MoveFromHeadNode(null);
            }
            //Moving head
            else
            {
                MoveFromHeadNode(current, IfChain);
            }
        }

        //Engages in a generic move from place to place
        private void GeneralMove(int fromIndex, int toIndex, Node fromNode, Node fromPrevious, Node toNode, Node IfChain)
        {

            if (fromIndex < toIndex && fromNode.Next != null && fromNode.Child != null && fromNode.Next.Index > toIndex) { }
            else if (fromIndex < toIndex && fromNode.Next == null && fromNode.Child != null) { }
            else if (fromIndex < toIndex && toNode.Next == null && addAfterBranchNode != null && (addAfterBranchNode.Next == null || addAfterBranchNode.Next.Index == (toIndex + 1)))
                MoveToAfterChildBranch(fromNode, fromPrevious, IfChain);
            else if ((fromIndex < toIndex && (fromPrevious.Child == fromNode) && (toNode.Child != null)) || ((fromPrevious.Child == fromNode) && (previousNode.Child == toNode)))
                MoveFromChildToChildNode(fromNode, fromPrevious, toNode, IfChain);
            else if (fromPrevious.Child == fromNode && fromNode.Next != null)
            {
                if (previousNode == toNode)
                    MoveFromChildNode(fromNode, fromPrevious, null, IfChain);
                else
                    MoveFromChildNode(fromNode, fromPrevious, toNode, IfChain);
            }
            else if ((fromIndex < toIndex && toNode.Child != null) || previousNode.Child == toNode)
                MoveToChildNode(fromNode, fromPrevious, toNode, IfChain);
            else
            {
                if (previousNode == toNode)
                    MoveNode(fromNode, fromPrevious, null, IfChain);
                else
                    MoveNode(fromNode, fromPrevious, toNode, IfChain);
            }
        }

        //finds the end of the linked else if / else to the given if statement
        //Pretty recursion
        private Node FindEndIf(Node current)
        {
            Block nextBlock = null;
            if (current.Next != null)
                nextBlock = current.Next.Data as Block;
            if (current.Next == null || !nextBlock.Text.Contains("ELSE") || nextBlock.flag_endIndent)
                return current;
            else
                return FindEndIf(current.Next);
        }

        //Find the end-if to determine exactly what kind of IF move we are doing here
        private Node CheckIfNodeMove(Node current, int index)
        {
            Node temp = FindEndIf(current);
            if (temp == null) //fail to find end if
                return null;
            else if (temp == current) // this is an empty IF
                return temp;
            else if (current.Index < index && temp.Child.Index == index)  //trying to add to after the if link
                return null;
            else if (current.Index < index && temp.Child.Index > index) //trying to move head if into linked else if / else statements
                return null;
            else   // free to move
                return temp;
        }

        //used to check to see if the current node is an IF to preform more checks for the move
        private bool isIF(Node current)
        {
            if ((current.Data as Block).Text.Equals("IF"))
            {
                return true;
            }
            return false;
        }

        private void MoveFromHeadNode(Node toNode, Node IfChain)
        {
            Node current = head;
            head = IfChain.Next;
            if (current.Index < toNode.Index)
            {
                IfChain.Next = toNode.Next;
                toNode.Next = current;
            }
            else
            {
                IfChain.Next = toNode;
                previousNode.Next = current;
            }
        }

        //works
        private void MoveHeadToChildNode(Node toNode, Node IfChain)
        {
            Node current = head;
            head = IfChain.Next;
            if (current.Index < toNode.Index)
            {
                IfChain.Next = toNode.Child;
                toNode.Child = current;
            }
            else
            {
                IfChain.Next = toNode;
                previousNode.Child = current;
            }

        }

        //works
        private void MoveToHeadNode(Node fromNode, Node IfChain)
        {
            previousNode.Next = IfChain.Next;
            IfChain.Next = head;
            head = fromNode;
        }

        //works
        private void MoveChildToHeadNode(Node fromNode, Node IfChain)
        {
            previousNode.Child = IfChain.Next;
            IfChain.Next = head;
            head = fromNode;
        }

        //works
        private void MoveHeadToAfterChildBranch(Node toNode, Node IfChain)
        {
            Node current = head;
            head = IfChain.Next;
            IfChain.Next = addAfterBranchNode.Next;
            addAfterBranchNode.Next = current;
        }

        //works
        private void MoveFromChildNode(Node fromNode, Node fromPrevious, Node toNode, Node IfChain)
        {
            fromPrevious.Child = IfChain.Next;
            if (fromNode.Index < toNode.Index)
            {
                IfChain.Next = toNode.Next;
                toNode.Next = fromNode;
            }
            else
            {
                IfChain.Next = toNode;
                previousNode.Next = fromNode;
            }
        }

        //works
        private void MoveToChildNode(Node fromNode, Node fromPrevious, Node toNode, Node IfChain)
        {
            fromPrevious.Next = IfChain.Next;
            if (fromNode.Index < toNode.Index)
            {
                IfChain.Next = toNode.Child;
                toNode.Child = fromNode;
            }
            else
            {
                IfChain.Next = toNode;
                previousNode.Child = fromNode;
            }
        }

        //works
        private void MoveFromChildToChildNode(Node fromNode, Node fromPrevious, Node toNode, Node IfChain)
        {
            fromPrevious.Child = IfChain.Next;
            if (fromNode.Index < toNode.Index)
            {
                IfChain.Next = toNode.Child;
                toNode.Child = fromNode;
            }
            else
            {
                IfChain.Next = toNode;
                previousNode.Child = fromNode;
            }
        }

        //works 
        private void MoveNode(Node fromNode, Node fromPrevious, Node toNode, Node IfChain)
        {
            fromPrevious.Next = IfChain.Next;
            if (fromNode.Index < toNode.Index)
            {
                IfChain.Next = toNode.Next;
                toNode.Next = fromNode;
            }
            else
            {
                IfChain.Next = toNode;
                previousNode.Next = fromNode;
            }

        }

        //works
        private void MoveToAfterChildBranch(Node fromNode, Node fromPrevious, Node IfChain)
        {
            if (fromPrevious.Child == fromNode)
            {
                fromPrevious.Child = IfChain.Next;
                IfChain.Next = addAfterBranchNode.Next;
                addAfterBranchNode.Next = fromNode;
            }
            else
            {
                fromPrevious.Next = IfChain.Next;
                IfChain.Next = addAfterBranchNode.Next;
                addAfterBranchNode.Next = fromNode;
            }
        }

        protected override int ResetIndex(Node current, int index)
        {
            while (current != null)
            {
                if (current.Data.GetType().Equals(typeof(Block)))
                {
                    Block temp = current.Data as Block;
                    if (temp.flag_endIndent)
                        marginSize -= 50;
                    temp.index = index;
                    temp.Margin = new Thickness(marginSize, 0, 0, 0);
                    returnList.Add(temp);
                }
                current.Index = index;
                index++;
                if (current.Child != null)
                {
                    if (current.Data.GetType().Equals(typeof(Block)))
                    {
                        Block temp = current.Data as Block;
                        if (temp.flag_beginIndent)
                            marginSize += 50;
                    }
                    index = ResetIndex(current.Child, index);

                }
                current = current.Next;
            }
            return index;
        }

        public ObservableCollection<Block> ListRefresh()
        {
            returnList = new ObservableCollection<Block>();
            ResetIndex(head, 0);
            return returnList;
        }

        public ObservableCollection<Block> clearList()
        {
            returnList = new ObservableCollection<Block>();
            head = null;
            return returnList;
        }

        public ReadOnlyObservableCollection<Block> GetBlocks()
        {
            ObservableCollection<Block> list = new ObservableCollection<Block>();
            return new ReadOnlyObservableCollection<Block>(GetBlocksList(head,list));
        }

        private ObservableCollection<Block> GetBlocksList(Node current, ObservableCollection<Block> list)
        {
            
            while (current != null)
            {
                list.Add(current.Data);
                if (current.Child != null)
                    GetBlocksList(current.Child, list);
                current = current.Next;
            }
            return list;
        }

        //switch the data of a node calls replaceData based on switching by name or by block type
        public ObservableCollection<Block> UpdateBlock(Block newBlock, bool name)
        {
            if (name)
                ReplaceNodeDataNameBased(head, newBlock);
            else
                ReplaceNodeData(head, newBlock);
            returnList = new ObservableCollection<Block>();
            ResetIndex(head, 0);
            return returnList;
        }

        //finds every block of the same type as newBlock and replaces the Node data with the newBlock
        private void ReplaceNodeDataNameBased(Node current, Block newBlock)
        {
            while (current != null)
            {
                //only blocks that have names are parameter, variable or method
                if (current.Data.Text.Equals("PARAMETER") || current.Data.Text.Equals("VARIABLE") || current.Data.Text.Equals("METHOD"))
                    if (current.Data.metadataList[1].Equals(newBlock.metadataList[1]))
                        current.Data = newBlock.cloneSelf(true);
                if (current.Child != null)
                    ReplaceNodeDataNameBased(current.Child, newBlock);
                current = current.Next;
            }
        }

        //finds every block of the same type as newBlock and replaces the Node data with the newBlock
        private void ReplaceNodeData(Node current, Block newBlock)
        {
            while (current != null)
            {
                if (!current.Data.Text.Contains("END"))
                    if (current.Data.metadataList[0].Equals(newBlock.metadataList[0]))
                        current.Data = newBlock;
                if (current.Child != null)
                    ReplaceNodeData(current.Child, newBlock);
                current = current.Next;
            }
        }
    }
}
