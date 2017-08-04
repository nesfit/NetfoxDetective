using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;

namespace Netfox.GUI.Detective.Core
{
    public interface IViewModelLocator
    {
        bool IsInDebug { get; }
        IEnumerable<string> GetDynamicMemberNames();
        DynamicMetaObject GetMetaObject(Expression parameter);
        //IConfigurationStore Store { get; set; }
        //IWindsorContainer Container { get; set; }
        void Initialize();
        void Install(IWindsorContainer container, IConfigurationStore store);
        bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result);
        bool TryConvert(ConvertBinder binder, out object result);
        bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result);
        bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes);
        bool TryDeleteMember(DeleteMemberBinder binder);
        bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result);
        bool TryGetMember(GetMemberBinder binder, out object result);
        bool TryInvoke(InvokeBinder binder, object[] args, out object result);
        bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result);
        bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value);
        bool TrySetMember(SetMemberBinder binder, object value);
        bool TryUnaryOperation(UnaryOperationBinder binder, out object result);
    }
}