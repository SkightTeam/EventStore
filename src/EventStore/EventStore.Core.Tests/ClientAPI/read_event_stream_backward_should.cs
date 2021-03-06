﻿// Copyright (c) 2012, Event Store LLP
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
using System.Linq;
using EventStore.ClientAPI;
using EventStore.Core.Tests.ClientAPI.Helpers;
using EventStore.Core.Tests.Helpers;
using NUnit.Framework;

namespace EventStore.Core.Tests.ClientAPI
{
    [TestFixture, Category("LongRunning")]
    public class read_event_stream_backward_should: SpecificationWithDirectoryPerTestFixture
    {
        private MiniNode _node;

        [TestFixtureSetUp]
        public override void TestFixtureSetUp()
        {
            base.TestFixtureSetUp();
            _node = new MiniNode(PathName);
            _node.Start();
        }

        [TestFixtureTearDown]
        public override void TestFixtureTearDown()
        {
            _node.Shutdown();
            base.TestFixtureTearDown();
        }

        [Test]
        [Category("Network")]
        public void throw_if_count_le_zero()
        {
            const string stream = "read_event_stream_backward_should_throw_if_count_le_zero";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();
                Assert.Throws<ArgumentOutOfRangeException>(() => store.ReadStreamEventsBackwardAsync(stream, 0, 0, resolveLinkTos: false));
            }
        }

        [Test]
        [Category("Network")]
        public void notify_using_status_code_if_stream_not_found()
        {
            const string stream = "read_event_stream_backward_should_notify_using_status_code_if_stream_not_found";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();
                var read = store.ReadStreamEventsBackwardAsync(stream, StreamPosition.End, 1, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);

                Assert.That(read.Result.Status, Is.EqualTo(SliceReadStatus.StreamNotFound));
            }
        }

        [Test]
        [Category("Network")]
        public void notify_using_status_code_if_stream_was_deleted()
        {
            const string stream = "read_event_stream_backward_should_notify_using_status_code_if_stream_was_deleted";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();
                var delete = store.DeleteStreamAsync(stream, ExpectedVersion.EmptyStream, hardDelete: true);
                Assert.DoesNotThrow(delete.Wait);

                var read = store.ReadStreamEventsBackwardAsync(stream, StreamPosition.End, 1, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);

                Assert.That(read.Result.Status, Is.EqualTo(SliceReadStatus.StreamDeleted));
            }
        }

        [Test]
        [Category("Network")]
        public void return_no_events_when_called_on_empty_stream()
        {
            const string stream = "read_event_stream_backward_should_return_single_event_when_called_on_empty_stream";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();

                var read = store.ReadStreamEventsBackwardAsync(stream, StreamPosition.End, 1, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);

                Assert.That(read.Result.Events.Length, Is.EqualTo(0));
            }
        }

        [Test]
        [Category("Network")]
        public void return_partial_slice_if_no_enough_events_in_stream()
        {
            const string stream = "read_event_stream_backward_should_return_partial_slice_if_no_enough_events_in_stream";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();

                var testEvents = Enumerable.Range(0, 10).Select(x => TestEvent.NewTestEvent((x + 1).ToString())).ToArray();
                var write10 = store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, testEvents);
                Assert.DoesNotThrow(write10.Wait);

                var read = store.ReadStreamEventsBackwardAsync(stream, 1, 5, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);

                Assert.That(read.Result.Events.Length, Is.EqualTo(2));
            }
        }

        [Test]
        [Category("Network")]
        public void return_events_reversed_compared_to_written()
        {
            const string stream = "read_event_stream_backward_should_return_events_reversed_compared_to_written";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();

                var testEvents = Enumerable.Range(0, 10).Select(x => TestEvent.NewTestEvent((x + 1).ToString())).ToArray();
                var write10 = store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, testEvents);
                Assert.DoesNotThrow(write10.Wait);

                var read = store.ReadStreamEventsBackwardAsync(stream, StreamPosition.End, testEvents.Length, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);

                Assert.That(EventDataComparer.Equal(testEvents.Reverse().ToArray(), read.Result.Events.Select(x => x.Event).ToArray()));
            }
        }

        [Test]
        [Category("Network")]
        public void be_able_to_read_single_event_from_arbitrary_position()
        {
            const string stream = "read_event_stream_backward_should_be_able_to_read_single_event_from_arbitrary_position";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();

                var testEvents = Enumerable.Range(0, 10).Select(x => TestEvent.NewTestEvent(x.ToString())).ToArray();
                var write10 = store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, testEvents);
                Assert.DoesNotThrow(write10.Wait);

                var read = store.ReadStreamEventsBackwardAsync(stream, 7, 1, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);

                Assert.That(EventDataComparer.Equal(testEvents[7], read.Result.Events.Single().Event));
            }
        }

        [Test]
        [Category("Network")]
        public void be_able_to_read_first_event()
        {
            const string stream = "read_event_stream_backward_should_be_able_to_read_first_event";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();

                var testEvents = Enumerable.Range(0, 10).Select(x => TestEvent.NewTestEvent((x + 1).ToString())).ToArray();
                var write10 = store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, testEvents);
                Assert.DoesNotThrow(write10.Wait);

                var read = store.ReadStreamEventsBackwardAsync(stream, StreamPosition.Start, 1, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);

                Assert.That(read.Result.Events.Length, Is.EqualTo(1));
            }
        }

        [Test]
        [Category("Network")]
        public void be_able_to_read_last_event()
        {
            const string stream = "read_event_stream_backward_should_be_able_to_read_last_event";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();

                var testEvents = Enumerable.Range(0, 10).Select(x => TestEvent.NewTestEvent(x.ToString())).ToArray();
                var write10 = store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, testEvents);
                Assert.DoesNotThrow(write10.Wait);

                var read = store.ReadStreamEventsBackwardAsync(stream, StreamPosition.End, 1, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);

                Assert.That(EventDataComparer.Equal(testEvents.Last(), read.Result.Events.Single().Event));
            }
        }

        [Test]
        [Category("Network")]
        public void be_able_to_read_slice_from_arbitrary_position()
        {
            const string stream = "read_event_stream_backward_should_be_able_to_read_slice_from_arbitrary_position";
            using (var store = TestConnection.Create(_node.TcpEndPoint))
            {
                store.Connect();

                var testEvents = Enumerable.Range(0, 10).Select(x => TestEvent.NewTestEvent(x.ToString())).ToArray();
                var write10 = store.AppendToStreamAsync(stream, ExpectedVersion.EmptyStream, testEvents);
                Assert.DoesNotThrow(write10.Wait);

                var read = store.ReadStreamEventsBackwardAsync(stream, 3, 2, resolveLinkTos: false);
                Assert.DoesNotThrow(read.Wait);

                Assert.That(EventDataComparer.Equal(testEvents.Skip(2).Take(2).Reverse().ToArray(), 
                                                     read.Result.Events.Select(x => x.Event).ToArray()));
            }
        }
    }
}
