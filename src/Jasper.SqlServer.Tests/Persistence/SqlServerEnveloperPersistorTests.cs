﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Baseline;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports;
using Jasper.SqlServer.Persistence;
using Shouldly;
using Xunit;

namespace Jasper.SqlServer.Tests.Persistence
{
    public class SqlServerEnveloperPersistorTests : ConnectedContext
    {
        private readonly SqlServerEnvelopePersistor thePersistor
            = new SqlServerEnvelopePersistor(new SqlServerSettings
            {
                ConnectionString = ConnectionSource.ConnectionString
            });


        /*
         * DeleteIncomingEnvelopes
         * DeleteOutgoingEnvelopes
         * MoveToDeadLetterStorage
         * ScheduleExecution
         * LoadDeadLetterEnvelope
         * IncrementIncomingEnvelopeAttempts
         * DiscardAndReassignOutgoing
         */

        [Fact]
        public async Task store_a_single_incoming_envelope()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Status = TransportConstants.Incoming;

            await thePersistor.StoreIncoming(envelope);

            var stored = thePersistor.AllIncomingEnvelopes().Single();

            stored.Id.ShouldBe(envelope.Id);
            stored.OwnerId.ShouldBe(envelope.OwnerId);
            stored.Status.ShouldBe(envelope.Status);
        }

        [Fact]
        public async Task store_multiple_incoming_envelopes()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());

            var stored = thePersistor.AllIncomingEnvelopes();

            list.Select(x => x.Id).OrderBy(x => x)
                .ShouldHaveTheSameElementsAs(stored.Select(x => x.Id).OrderBy(x => x));
        }

        [Fact]
        public async Task store_a_single_outgoing_envelope()
        {
            var envelope = ObjectMother.Envelope();
            envelope.Status = TransportConstants.Outgoing;

            await thePersistor.StoreOutgoing(envelope, 5890);

            var stored = thePersistor.AllOutgoingEnvelopes().Single();

            stored.Id.ShouldBe(envelope.Id);
            stored.OwnerId.ShouldBe(5890);
            stored.Status.ShouldBe(envelope.Status);
        }

        [Fact]
        public async Task store_multiple_outgoing_envelopes()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Outgoing;

                list.Add(envelope);
            }

            await thePersistor.StoreOutgoing(list.ToArray(), 111);

            var stored = thePersistor.AllOutgoingEnvelopes();

            list.Select(x => x.Id).OrderBy(x => x)
                .ShouldHaveTheSameElementsAs(stored.Select(x => x.Id).OrderBy(x => x));

            stored.Each(x => x.OwnerId.ShouldBe(111));
        }

        [Fact]
        public async Task delete_a_single_outgoing_envelope()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Outgoing;

                list.Add(envelope);
            }

            await thePersistor.StoreOutgoing(list.ToArray(), 111);

            var toDelete = list[5];

            await thePersistor.DeleteOutgoingEnvelope(toDelete);

            var stored = thePersistor.AllOutgoingEnvelopes();
            stored.Count.ShouldBe(9);

            stored.Any(x => x.Id == toDelete.Id).ShouldBeFalse();

        }

        [Fact]
        public async Task delete_multiple_outgoing_envelope()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Outgoing;

                list.Add(envelope);
            }

            await thePersistor.StoreOutgoing(list.ToArray(), 111);

            var toDelete = new Envelope[] {list[2], list[3], list[7]};

            await thePersistor.DeleteOutgoingEnvelopes(toDelete);

            var stored = thePersistor.AllOutgoingEnvelopes();
            stored.Count.ShouldBe(7);

            stored.Any(x => x.Id == list[2].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[3].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[7].Id).ShouldBeFalse();

        }

        [Fact]
        public async Task delete_multiple_incoming_envelope()
        {
            var list = new List<Envelope>();

            for (int i = 0; i < 10; i++)
            {
                var envelope = ObjectMother.Envelope();
                envelope.Status = TransportConstants.Incoming;

                list.Add(envelope);
            }

            await thePersistor.StoreIncoming(list.ToArray());

            var toDelete = new Envelope[] {list[2], list[3], list[7]};

            await thePersistor.DeleteIncomingEnvelopes(toDelete);

            var stored = thePersistor.AllIncomingEnvelopes();
            stored.Count.ShouldBe(7);

            stored.Any(x => x.Id == list[2].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[3].Id).ShouldBeFalse();
            stored.Any(x => x.Id == list[7].Id).ShouldBeFalse();

        }
    }
}
