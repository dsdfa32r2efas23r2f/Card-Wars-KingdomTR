using NUnit.Framework;
using Project.Core.Application.Meta.Commands;
using Project.Core.Application.Meta.UseCases;
using Project.Core.Domain.Meta.Entities;
using Project.Core.Domain.Meta.ValueObjects;

// ─── Mission ──────────────────────────────────────────────────────────────

namespace Project.Tests.EditMode
{
    [TestFixture]
    public class ApplyGrantCurrencyServiceTests
    {
        private ApplyGrantCurrencyService _service;

        [SetUp]
        public void SetUp() => _service = new ApplyGrantCurrencyService();

        [Test]
        public void Grant_AddsAmountsToEmptyWallet()
        {
            var amounts = new CurrencyAmounts(10, 20, 5, 100, 3);
            var command = new GrantCurrencyCommand("store", amounts);

            MetaWalletState result = _service.Apply(MetaWalletState.Empty, command);

            Assert.AreEqual(10, result.PaidHardCurrency);
            Assert.AreEqual(20, result.FreeHardCurrency);
            Assert.AreEqual(30, result.HardCurrency);
            Assert.AreEqual(5, result.PvpCurrency);
            Assert.AreEqual(100, result.SoftCurrency);
            Assert.AreEqual(3, result.CustomizationCurrency);
        }

        [Test]
        public void Grant_AccumulatesOnExistingState()
        {
            var initial = new MetaWalletState(new CurrencyAmounts(10, 10, 0, 50, 0));
            var command = new GrantCurrencyCommand("store", new CurrencyAmounts(5, 15, 0, 25, 0));

            MetaWalletState result = _service.Apply(initial, command);

            Assert.AreEqual(15, result.PaidHardCurrency);
            Assert.AreEqual(25, result.FreeHardCurrency);
            Assert.AreEqual(75, result.SoftCurrency);
        }

        [Test]
        public void Grant_NullCommand_ReturnsCurrent()
        {
            var initial = new MetaWalletState(new CurrencyAmounts(5, 5, 0, 0, 0));

            MetaWalletState result = _service.Apply(initial, null);

            Assert.AreEqual(10, result.HardCurrency);
        }

        [Test]
        public void Grant_NullState_TreatsAsEmpty()
        {
            var command = new GrantCurrencyCommand("store", new CurrencyAmounts(0, 50, 0, 0, 0));

            MetaWalletState result = _service.Apply(null, command);

            Assert.AreEqual(50, result.FreeHardCurrency);
        }
    }

    [TestFixture]
    public class ApplySpendCurrencyServiceTests
    {
        private ApplySpendCurrencyService _service;

        [SetUp]
        public void SetUp() => _service = new ApplySpendCurrencyService();

        [Test]
        public void Spend_SucceedsWhenAffordable()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 100, 0, 0, 0));
            var command = new SpendCurrencyCommand("stamina_refill", new SpendAmounts(50, 0, 0, 0));

            SpendResult result = _service.Apply(state, command);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(50, result.NewState.HardCurrency);
        }

        [Test]
        public void Spend_FailsWhenInsufficientFunds()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 10, 0, 0, 0));
            var command = new SpendCurrencyCommand("stamina_refill", new SpendAmounts(50, 0, 0, 0));

            SpendResult result = _service.Apply(state, command);

            Assert.IsFalse(result.Succeeded);
            Assert.IsNotEmpty(result.FailureReason);
            Assert.AreEqual(10, result.NewState.HardCurrency);
        }

        [Test]
        public void Spend_NullCommand_ReturnsOkWithSameState()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 50, 0, 0, 0));

            SpendResult result = _service.Apply(state, null);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(50, result.NewState.HardCurrency);
        }
    }

    [TestFixture]
    public class MetaWalletStateSpendTests
    {
        [Test]
        public void Spend_DeductsFreeFirst()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 100, 0, 0, 0));

            MetaWalletState result = state.Spend(new SpendAmounts(60, 0, 0, 0));

            Assert.AreEqual(0, result.PaidHardCurrency);
            Assert.AreEqual(40, result.FreeHardCurrency);
        }

        [Test]
        public void Spend_AfterFreeExhausted_DeductsPaid()
        {
            // 70 paid + 30 free = 100 total, spend 50: free goes to 0, paid goes to 50
            var state = new MetaWalletState(new CurrencyAmounts(70, 30, 0, 0, 0));

            MetaWalletState result = state.Spend(new SpendAmounts(50, 0, 0, 0));

            Assert.AreEqual(0, result.FreeHardCurrency);
            Assert.AreEqual(50, result.PaidHardCurrency);
        }

        [Test]
        public void Spend_NoFree_DeductsPaidOnly()
        {
            var state = new MetaWalletState(new CurrencyAmounts(100, 0, 0, 0, 0));

            MetaWalletState result = state.Spend(new SpendAmounts(40, 0, 0, 0));

            Assert.AreEqual(60, result.PaidHardCurrency);
            Assert.AreEqual(0, result.FreeHardCurrency);
        }

        [Test]
        public void Spend_OtherCurrenciesDeductedIndependently()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 0, 50, 200, 10));

            MetaWalletState result = state.Spend(new SpendAmounts(0, 20, 100, 5));

            Assert.AreEqual(30, result.PvpCurrency);
            Assert.AreEqual(100, result.SoftCurrency);
            Assert.AreEqual(5, result.CustomizationCurrency);
        }

        [Test]
        public void CanAfford_TrueWhenExactAmount()
        {
            var state = new MetaWalletState(new CurrencyAmounts(10, 40, 0, 0, 0));

            Assert.IsTrue(state.CanAfford(new SpendAmounts(50, 0, 0, 0)));
        }

        [Test]
        public void CanAfford_FalseWhenInsufficient()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 30, 0, 0, 0));

            Assert.IsFalse(state.CanAfford(new SpendAmounts(50, 0, 0, 0)));
        }
    }

    [TestFixture]
    public class ApplyClaimMissionServiceTests
    {
        private ApplyClaimMissionService _service;

        [SetUp]
        public void SetUp() => _service = new ApplyClaimMissionService();

        [Test]
        public void Claim_GrantsRewardsToWallet()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 10, 5, 100, 0));
            var rewards = new CurrencyAmounts(0, 20, 10, 50, 0);
            var command = new ClaimMissionCommand("mission_001", rewards);

            MetaWalletState result = _service.Apply(state, command);

            Assert.AreEqual(30, result.HardCurrency);
            Assert.AreEqual(15, result.PvpCurrency);
            Assert.AreEqual(150, result.SoftCurrency);
        }

        [Test]
        public void Claim_NullCommand_ReturnsCurrent()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 50, 0, 0, 0));

            MetaWalletState result = _service.Apply(state, null);

            Assert.AreEqual(50, result.HardCurrency);
        }

        [Test]
        public void Claim_NullState_TreatsAsEmpty()
        {
            var rewards = new CurrencyAmounts(0, 30, 0, 0, 0);
            var command = new ClaimMissionCommand("mission_002", rewards);

            MetaWalletState result = _service.Apply(null, command);

            Assert.AreEqual(30, result.HardCurrency);
        }
    }

    [TestFixture]
    public class ApplyRefillStaminaServiceTests
    {
        private ApplyRefillStaminaService _service;

        [SetUp]
        public void SetUp() => _service = new ApplyRefillStaminaService();

        [Test]
        public void Refill_SpendsCostFromWallet()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 100, 0, 0, 0));
            var command = new RefillStaminaCommand("stamina_refill", 30);

            SpendResult result = _service.Apply(state, command);

            Assert.IsTrue(result.Succeeded);
            Assert.AreEqual(70, result.NewState.HardCurrency);
        }

        [Test]
        public void Refill_FailsWhenInsufficientFunds()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 10, 0, 0, 0));
            var command = new RefillStaminaCommand("stamina_refill", 30);

            SpendResult result = _service.Apply(state, command);

            Assert.IsFalse(result.Succeeded);
            Assert.AreEqual(10, result.NewState.HardCurrency);
        }

        [Test]
        public void Refill_NullCommand_ReturnsOk()
        {
            var state = new MetaWalletState(new CurrencyAmounts(0, 50, 0, 0, 0));

            SpendResult result = _service.Apply(state, null);

            Assert.IsTrue(result.Succeeded);
        }
    }

    [TestFixture]
    public class MissionStateModelTests
    {
        [Test]
        public void MarkClaimed_SetsIsClaimed()
        {
            var mission = new MissionStateModel("mission_001", isCompleted: true, isClaimed: false);

            MissionStateModel claimed = mission.MarkClaimed();

            Assert.IsTrue(claimed.IsClaimed);
            Assert.AreEqual("mission_001", claimed.MissionId);
            Assert.IsTrue(claimed.IsCompleted);
        }

        [Test]
        public void MarkClaimed_IsImmutable()
        {
            var mission = new MissionStateModel("mission_001", isCompleted: true, isClaimed: false);

            mission.MarkClaimed();

            Assert.IsFalse(mission.IsClaimed);
        }
    }
}
