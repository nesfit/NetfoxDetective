// Copyright (c) 2017 Jan Pluskal, Filip Karpisek
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Netfox.SnooperBTC.Models
{
	public enum BTCTransactionType
	{
		Unknown,
		Output,
		Input
	}
    [ComplexType]
    public class BTCTransaction
    {
	    public BTCTransactionType Type = BTCTransactionType.Unknown;
		// common transaction content
		public uint ScriptLength { get; private set; } = 0;
		public byte[] SignatureScript { get; private set; } = null;
		// input transaction content
		public byte[] PreviousOutpoint { get; private set; } = null;
		public byte[] Sequence { get; private set; } = null;
		// output transaction content
	    public byte[] Value { get; private set; } = null;
		internal bool Parse(uint startIndex, byte[] inputArray, out uint endIndex)
	    {
		    endIndex = startIndex;
			switch(this.Type)
			{
				case BTCTransactionType.Input:
					if(inputArray.Length < endIndex + 36) { return false; }
					this.PreviousOutpoint = new byte[36];
					Array.Copy(inputArray, startIndex, this.PreviousOutpoint, 0, 36);
					endIndex = startIndex + 36;
					this.ScriptLength = (uint) BTCMsg.ParseVarInt(endIndex, inputArray, out endIndex);
					if(inputArray.Length < endIndex + this.ScriptLength) { return false; }
					this.SignatureScript = new byte[this.ScriptLength];
					Array.Copy(inputArray, endIndex, this.SignatureScript, 0, this.ScriptLength);
					endIndex += this.ScriptLength;
					if(inputArray.Length < endIndex + 4) { return false; }
					this.Sequence = new byte[4];
                    Array.Copy(inputArray, endIndex, this.Sequence, 0, 4);
					endIndex += 4;
					return true;
				case BTCTransactionType.Output:
					if (inputArray.Length < endIndex + 8) { return false; }
					this.Value = new byte[8];
					Array.Copy(inputArray, endIndex, this.Value, 0, 8);
					endIndex += 8;
					this.ScriptLength = (uint)BTCMsg.ParseVarInt(endIndex, inputArray, out endIndex);
					if (inputArray.Length < endIndex + this.ScriptLength) { return false; }
					this.SignatureScript = new byte[this.ScriptLength];
					Array.Copy(inputArray, endIndex, this.SignatureScript, 0, this.ScriptLength);
					endIndex += this.ScriptLength;
					return true;
				default:
					return false;
			}
	    }
    }
}
