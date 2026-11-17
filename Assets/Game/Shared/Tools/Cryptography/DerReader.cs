using System;

namespace Shared.Tools.Cryptography
{
    public class DerReader
    {
        private const int LongLengthFlag = 0x80;
        private const int LengthBitsMask = 0x7F;
        private const int BitsInByte = 8;

        private readonly byte[] _data;
        private int _position;

        public DerReader(byte[] data)
        {
            _data = data;
        }

        public DerReader ReadSequence(byte tag)
        {
            return new DerReader(ReadElement(tag));
        }

        public byte[] ReadElement(byte tag)
        {
            if (_position >= _data.Length || _data[_position] != tag)
            {
                throw new FormatException($"DER element with tag {tag} expected at position {_position}");
            }

            _position++;

            var length = ReadLength();

            if (length < 0 || _position + length > _data.Length)
            {
                throw new FormatException($"DER element length {length} is out of range");
            }

            var content = new byte[length];
            Array.Copy(_data, _position, content, 0, length);
            _position += length;

            return content;
        }

        private int ReadLength()
        {
            var first = _data[_position];
            _position++;

            if ((first & LongLengthFlag) == 0)
            {
                return first;
            }

            var lengthBytesCount = first & LengthBitsMask;
            var length = 0;

            for (var index = 0; index < lengthBytesCount; index++)
            {
                length = (length << BitsInByte) | _data[_position];
                _position++;
            }

            return length;
        }
    }
}
