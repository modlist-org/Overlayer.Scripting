using Acornima.Ast;
using Jint;
using Jint.Native;
using JSNet.Utils;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Collections.Generic;

namespace Overlayer.Scripting;

public static class Expression {
    public static readonly Dictionary<string, ExprContext> expressions = new();

    [Tag("Expression", NotPlaying = true)]
    public static object Expr(string expr) {
        if(expressions.TryGetValue(expr, out var res)) {
            return res == null || !res.prepared.IsValid ? null : (object)res.Run();
        }

        var prepared = Engine.PrepareScript(JSUtils.RemoveImports(expr));

        if(!prepared.IsValid) {
            expressions[expr] = new ExprContext(null, prepared);
            return null;
        }

        var engine = Main.JSApi.PrepareInterpreter();
        var ctx = new ExprContext(engine, prepared);
        expressions[expr] = ctx;

        return ctx.Run();
    }

    public class ExprContext {
        public Engine engine;
        public Prepared<Script> prepared;
        public bool IsFaulted;

        public ExprContext(Engine engine, Prepared<Script> prepared) {
            this.engine = engine;
            this.prepared = prepared;
        }

        public JsValue Run() {
            if(IsFaulted || engine == null || !prepared.IsValid) {
                return JsValue.Null;
            }

            return MiscUtils.ExecuteSafe(() => engine.Evaluate(prepared), out var ex) ?? JsValue.Null;
        }
    }
}