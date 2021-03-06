﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Interpritator.Source.Extension;
using Interpritator.Source.Interpritator.Command;

namespace Interpritator.Source.Interpritator
{
    public class NumberCommandInterpritator
    {
        private static List<Func<NumberCommand, object>> _commandList = new List<Func<NumberCommand, object>>
        {
            NumberCommand.GetOperandsList,
            NumberCommand.NotFirst,
            NumberCommand.Or,
            NumberCommand.And,
            NumberCommand.Xor,
            NumberCommand.Impication,
            NumberCommand.CoImpication,
            NumberCommand.Equivalence,
            NumberCommand.Pierce,
            NumberCommand.Scheffer,
            NumberCommand.Addition,
            NumberCommand.Subtraction,
            NumberCommand.Multiplication,
            NumberCommand.StrongDivision,
            NumberCommand.Mod,
            NumberCommand.Swap,
            NumberCommand.Insert,
            NumberCommand.Convert,
            ReadInBase,
            NumberCommand.FindMaxDivider,
            NumberCommand.ShiftL,
            NumberCommand.ShiftR,
            NumberCommand.CycleShiftL,
            NumberCommand.CycleShiftR,
            NumberCommand.Copy
        };

        public static KeyValuePair<string, string> ExecuteProgram(string binFilePatch)
        {
            var binFile = File.Open(binFilePatch, FileMode.Open);


            var result = new StringBuilder();
            var errors = new StringBuilder();

            using (var br = new BinaryReader(binFile))
            {
                while (br.PeekChar() > -1)
                {
                    var intCommand = br.ReadInt32();
                    var bitCommand = BitArrayExtension.IntToBitArr(intCommand);

                    var commandResult = RunCommand(bitCommand, false);
                    result.Append(commandResult.Key);
                    errors.Append(commandResult.Value);
                }
            }

            binFile.Dispose();

            return new KeyValuePair<string, string>(result.ToString(), errors.ToString());
        }

        public static string GetCommandResult(NumberCommand command, bool isBin = false)
        {
            var operation = command.GetOperator();
            var operationNumber = operation.ToInt();


            var resultObj = _commandList[operationNumber].Invoke(command);

            if (resultObj is NumberCommand isNumberCommandResult)
            {
                var numCommandResult = (NumberCommand) resultObj;
               return (isBin)? numCommandResult.ToBinStr() : numCommandResult.ToString();
            }
            return resultObj as string;
             
        }

        public static KeyValuePair<string, string> RunCommand(BitArray bitCommand, bool isBinResult = false)
        {
            var result = new StringBuilder();
            var errors = new StringBuilder();

            try
            {
                var command = new NumberCommand(bitCommand);
                var commandCopy = new NumberCommand(bitCommand);

                var commandResult = GetCommandResult(command, isBinResult);

                var oldCommandStr = isBinResult ? commandCopy.ToBinStr() : commandCopy.ToString();
                result.Append(oldCommandStr  + " ---> " + commandResult + "\n");
            }
            catch (Exception e)
            {
                errors.Append(e + "\n");
            }

            return new KeyValuePair<string, string>(result.ToString(), errors.ToString());
        }


        private static string ReadInBase(NumberCommand command)
        {
            return command.ToBinStr();
        }

    }
}