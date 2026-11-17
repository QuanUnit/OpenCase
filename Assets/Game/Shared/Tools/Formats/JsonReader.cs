using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Shared.Tools.Formats
{
    public class JsonReader
    {
        private const int UnicodeEscapeLength = 4;

        private readonly string _text;
        private int _position;

        public JsonReader(string text)
        {
            _text = text;
        }

        public object Read()
        {
            var value = ReadValue();

            SkipWhitespace();

            if (_position < _text.Length)
            {
                throw CreateException("Unexpected trailing characters");
            }

            return value;
        }

        private object ReadValue()
        {
            SkipWhitespace();

            if (_position >= _text.Length)
            {
                throw CreateException("Unexpected end of JSON");
            }

            var character = _text[_position];

            switch (character)
            {
                case '{':
                    return ReadObject();
                case '[':
                    return ReadArray();
                case '"':
                    return ReadString();
                case 't':
                    ReadLiteral("true");
                    return true;
                case 'f':
                    ReadLiteral("false");
                    return false;
                case 'n':
                    ReadLiteral("null");
                    return null;
                default:
                    return ReadNumber();
            }
        }

        private Dictionary<string, object> ReadObject()
        {
            var result = new Dictionary<string, object>();
            _position++;

            SkipWhitespace();

            if (TryConsume('}') == true)
            {
                return result;
            }

            while (true)
            {
                SkipWhitespace();

                var key = ReadString();

                SkipWhitespace();
                Expect(':');

                result[key] = ReadValue();

                SkipWhitespace();

                if (TryConsume('}') == true)
                {
                    return result;
                }

                Expect(',');
            }
        }

        private List<object> ReadArray()
        {
            var result = new List<object>();
            _position++;

            SkipWhitespace();

            if (TryConsume(']') == true)
            {
                return result;
            }

            while (true)
            {
                result.Add(ReadValue());

                SkipWhitespace();

                if (TryConsume(']') == true)
                {
                    return result;
                }

                Expect(',');
            }
        }

        private string ReadString()
        {
            Expect('"');

            var builder = new StringBuilder();

            while (true)
            {
                if (_position >= _text.Length)
                {
                    throw CreateException("Unterminated string");
                }

                var character = _text[_position];
                _position++;

                if (character == '"')
                {
                    return builder.ToString();
                }

                if (character != '\\')
                {
                    builder.Append(character);
                    continue;
                }

                builder.Append(ReadEscapedCharacter());
            }
        }

        private char ReadEscapedCharacter()
        {
            if (_position >= _text.Length)
            {
                throw CreateException("Unterminated escape sequence");
            }

            var escaped = _text[_position];
            _position++;

            switch (escaped)
            {
                case '"':
                    return '"';
                case '\\':
                    return '\\';
                case '/':
                    return '/';
                case 'b':
                    return '\b';
                case 'f':
                    return '\f';
                case 'n':
                    return '\n';
                case 'r':
                    return '\r';
                case 't':
                    return '\t';
                case 'u':
                    return ReadUnicodeCharacter();
                default:
                    throw CreateException($"Unknown escape sequence \\{escaped}");
            }
        }

        private char ReadUnicodeCharacter()
        {
            if (_position + UnicodeEscapeLength > _text.Length)
            {
                throw CreateException("Unterminated unicode escape");
            }

            var hex = _text.Substring(_position, UnicodeEscapeLength);
            _position += UnicodeEscapeLength;

            return (char)int.Parse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
        }

        private double ReadNumber()
        {
            var start = _position;

            while (_position < _text.Length && IsNumberCharacter(_text[_position]) == true)
            {
                _position++;
            }

            if (start == _position)
            {
                throw CreateException($"Unexpected character '{_text[start]}'");
            }

            return double.Parse(_text.Substring(start, _position - start), NumberStyles.Float, CultureInfo.InvariantCulture);
        }

        private bool IsNumberCharacter(char character)
        {
            return char.IsDigit(character) == true || character == '-' || character == '+' || character == '.' || character == 'e' || character == 'E';
        }

        private void ReadLiteral(string literal)
        {
            if (string.CompareOrdinal(_text, _position, literal, 0, literal.Length) != 0)
            {
                throw CreateException($"Expected {literal}");
            }

            _position += literal.Length;
        }

        private void SkipWhitespace()
        {
            while (_position < _text.Length && char.IsWhiteSpace(_text[_position]) == true)
            {
                _position++;
            }
        }

        private bool TryConsume(char character)
        {
            if (_position < _text.Length && _text[_position] == character)
            {
                _position++;
                return true;
            }

            return false;
        }

        private void Expect(char character)
        {
            if (TryConsume(character) == false)
            {
                throw CreateException($"Expected '{character}'");
            }
        }

        private FormatException CreateException(string message)
        {
            return new FormatException($"{message} at position {_position}");
        }
    }
}
