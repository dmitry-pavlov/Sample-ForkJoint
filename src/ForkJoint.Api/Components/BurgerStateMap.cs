namespace ForkJoint.Api.Components
{
    using Contracts;
    using MassTransit;
    using MassTransit.EntityFrameworkCoreIntegration;
    using MassTransit.EntityFrameworkCoreIntegration.Mappings;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using StateMachines;


    public class BurgerStateMap :
        SagaClassMap<BurgerState>
    {
        protected override void Configure(EntityTypeBuilder<BurgerState> entity, ModelBuilder model)
        {
            entity.Property(x => x.CurrentState);

            entity.Property(x => x.Created);
            entity.Property(x => x.Completed);
            entity.Property(x => x.Faulted);

            entity.Property(x => x.ExceptionInfo).HasConversion(new JsonValueConverter<ExceptionInfo>())
                .Metadata.SetValueComparer(new JsonValueComparer<ExceptionInfo>());

            entity.Property(x => x.Burger)
                .HasConversion(new JsonValueConverter<Burger>())
                .Metadata.SetValueComparer(new JsonValueComparer<Burger>());

            entity.Property(x => x.RequestId);
            entity.Property(x => x.ResponseAddress);
        }
    }
}