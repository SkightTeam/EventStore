// Copyright (c) 2012, Event Store LLP
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are
// met:
// 
// Redistributions of source code must retain the above copyright notice,
// this list of conditions and the following disclaimer.
// Redistributions in binary form must reproduce the above copyright
// notice, this list of conditions and the following disclaimer in the
// documentation and/or other materials provided with the distribution.
// Neither the name of the Event Store LLP nor the names of its
// contributors may be used to endorse or promote products derived from
// this software without specific prior written permission
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
// "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
// LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
// A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
// HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
// SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
// LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
// DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
// THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
// OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using EventStore.Core.Helpers;
using EventStore.Core.Messaging;
using EventStore.Core.Tests.Fakes;
using EventStore.Projections.Core.Services.Processing;
using EventStore.Projections.Core.Tests.Services.core_projection;
using NUnit.Framework;

namespace EventStore.Projections.Core.Tests.Services.emitted_stream
{
    [TestFixture]
    public class when_creating_an_emitted_stream
    {
        private FakePublisher _fakePublisher;
        private IODispatcher _ioDispatcher;


        [SetUp]
        public void setup()
        {
            _fakePublisher = new FakePublisher();
            _ioDispatcher = new IODispatcher(_fakePublisher, new PublishEnvelope(_fakePublisher));
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void null_stream_id_throws_argument_null_exception()
        {
            new EmittedStream(
                null, new EmittedStream.WriterConfiguration(new EmittedStream.WriterConfiguration.StreamMetadata(), null, 50), new ProjectionVersion(1, 0, 0),
                new TransactionFilePositionTagger(0), CheckpointTag.FromPosition(0, 0, -1), _ioDispatcher,
                new TestCheckpointManagerMessageHandler());
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void null_writer_configuration_throws_argument_null_exception()
        {
            new EmittedStream(
                null, null, new ProjectionVersion(1, 0, 0),
                new TransactionFilePositionTagger(0), CheckpointTag.FromPosition(0, 0, -1), _ioDispatcher,
                new TestCheckpointManagerMessageHandler());
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void empty_stream_id_throws_argument_exception()
        {
            new EmittedStream(
                "", new EmittedStream.WriterConfiguration(new EmittedStream.WriterConfiguration.StreamMetadata(), null, 50), new ProjectionVersion(1, 0, 0),
                new TransactionFilePositionTagger(0), CheckpointTag.FromPosition(0, 0, -1), _ioDispatcher,
                new TestCheckpointManagerMessageHandler());
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void null_from_throws_argument_exception()
        {
            new EmittedStream(
                "", new EmittedStream.WriterConfiguration(new EmittedStream.WriterConfiguration.StreamMetadata(), null, 50), new ProjectionVersion(1, 0, 0),
                new TransactionFilePositionTagger(0), null, _ioDispatcher, new TestCheckpointManagerMessageHandler());
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void null_io_dispatcher_throws_argument_null_exception()
        {
            new EmittedStream(
                "test", new EmittedStream.WriterConfiguration(new EmittedStream.WriterConfiguration.StreamMetadata(), null, 50), new ProjectionVersion(1, 0, 0),
                new TransactionFilePositionTagger(0), CheckpointTag.FromPosition(0, 0, -1), null,
                new TestCheckpointManagerMessageHandler());
        }

        [Test, ExpectedException(typeof (ArgumentNullException))]
        public void null_ready_handler_throws_argumenbt_null_exception()
        {
            new EmittedStream(
                "test", new EmittedStream.WriterConfiguration(new EmittedStream.WriterConfiguration.StreamMetadata(), null, 50), new ProjectionVersion(1, 0, 0),
                new TransactionFilePositionTagger(0), CheckpointTag.FromPosition(0, 0, -1), _ioDispatcher, null);
        }

        [Test]
        public void it_can_be_created()
        {
            new EmittedStream(
                "test", new EmittedStream.WriterConfiguration(new EmittedStream.WriterConfiguration.StreamMetadata(), null, 50), new ProjectionVersion(1, 0, 0),
                new TransactionFilePositionTagger(0), CheckpointTag.FromPosition(0, 0, -1), _ioDispatcher,
                new TestCheckpointManagerMessageHandler());
        }
    }
}
