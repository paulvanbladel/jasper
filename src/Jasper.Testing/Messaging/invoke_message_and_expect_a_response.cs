﻿using System;
using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Messaging
{
    public class invoke_message_and_expect_a_response : SendingContext
    {

        [Fact]
        public async Task invoke_expecting_a_response()
        {
            await StartTheReceiver(x => { x.Handlers.IncludeType<QuestionAndAnswer>(); });

            var answer = await theReceiver.Messaging.Invoke<Answer>(new Question {One = 3, Two = 4});

            answer.Sum.ShouldBe(7);
            answer.Product.ShouldBe(12);
        }

        [Fact]
        public async Task invoke_with_no_known_response_do_not_blow_up()
        {
            await StartTheReceiver(x => { x.Handlers.IncludeType<QuestionAndAnswer>(); });
            (await theReceiver.Messaging.Invoke<Answer>(new QuestionWithNoAnswer()))
                .ShouldBeNull();
        }

        [Fact]
        public async Task invoke_with_expected_response_when_there_is_no_receiver()
        {
            await StartTheReceiver(x => { x.Handlers.IncludeType<QuestionAndAnswer>(); });
            await Exception<ArgumentOutOfRangeException>.ShouldBeThrownByAsync(async () =>
            {
                await theReceiver.Messaging.Invoke<Answer>(new QuestionWithNoHandler());
            });

        }
    }

    public class Question
    {
        public int One { get; set; }
        public int Two { get; set; }
    }

    public class Answer
    {
        public int Sum { get; set; }
        public int Product { get; set; }
    }

    public class QuestionWithNoHandler
    {

    }

    public class QuestionWithNoAnswer
    {

    }

    public class QuestionAndAnswer
    {
        public Answer Handle(Question question)
        {
            return new Answer
            {
                Sum = question.One + question.Two,
                Product = question.One * question.Two
            };
        }

        public void Handle(QuestionWithNoAnswer question)
        {

        }
    }
}
