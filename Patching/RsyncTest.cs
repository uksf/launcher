using System.IO;
using FastRsync.Core;
using FastRsync.Delta;
using FastRsync.Diagnostics;
using FastRsync.Signature;

namespace Patching {
    public class RsyncTest {
        public static void CalculateSignature(string filePath, string signaturePath) {
            if (!File.Exists(filePath)) {
                throw new FileNotFoundException();
            }
            SignatureBuilder signatureBuilder = new SignatureBuilder();
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream signatureStream = new FileStream(signaturePath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    signatureBuilder.Build(fileStream, new SignatureWriter(signatureStream));
                }
            }
        }

        public static void CalculateDelta(string updatedPath, string signaturePath, string deltaPath) {
            DeltaBuilder delta = new DeltaBuilder {ProgressReport = new ConsoleProgressReporter()};
            using (FileStream updatedStream = new FileStream(updatedPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream signatureStream = new FileStream(signaturePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream deltaStream = new FileStream(deltaPath, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                        delta.BuildDelta(updatedStream, new SignatureReader(signatureStream, delta.ProgressReport),
                                         new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream)));
                    }
                }
            }
        }

        public static void ApplyDelta(string updatedPath, string deltaPath, string originalPath) {
            DeltaApplier delta = new DeltaApplier {SkipHashCheck = true};
            using (FileStream updatedStream = new FileStream(updatedPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream deltaStream = new FileStream(deltaPath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream originalStream = new FileStream(originalPath, FileMode.Create, FileAccess.ReadWrite, FileShare.Read)) {
                        delta.Apply(updatedStream, new BinaryDeltaReader(deltaStream, new ConsoleProgressReporter()), originalStream);
                    }
                }
            }
        }

        public static void All() {
            string pathBasis = @"C:\Users\timbe\Desktop\test\uksf_air.pbo";
            string pathSignature = @"C:\Users\timbe\Desktop\test\uksf_air.pbo.sig";
            string pathDelta = @"C:\Users\timbe\Desktop\test\uksf_air.pbo.del";
            string pathNew = @"C:\Users\timbe\Desktop\test\uksf_air_updated.pbo";
            
            SignatureBuilder signatureBuilder = new SignatureBuilder();
            using (FileStream basisStream = new FileStream(pathBasis, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream signatureStream = new FileStream(pathSignature, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                    signatureBuilder.Build(basisStream, new SignatureWriter(signatureStream));
                }
            }
            DeltaBuilder deltaBuilder = new DeltaBuilder { ProgressReport = new ConsoleProgressReporter() };
            
            using (FileStream newFileStream = new FileStream(pathNew, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream signatureStream = new FileStream(pathSignature, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream deltaStream = new FileStream(pathDelta, FileMode.Create, FileAccess.Write, FileShare.Read)) {
                        deltaBuilder.BuildDelta(newFileStream, new SignatureReader(signatureStream, deltaBuilder.ProgressReport),
                                         new AggregateCopyOperationsDecorator(new BinaryDeltaWriter(deltaStream)));
                    }
                }
            }
            DeltaApplier deltaApplier = new DeltaApplier { SkipHashCheck = true };
            using (FileStream basisStream = new FileStream(pathNew, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                using (FileStream deltaStream = new FileStream(pathDelta, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    using (FileStream newFileStream = new FileStream(pathBasis, FileMode.Create, FileAccess.ReadWrite, FileShare.Read)) {
                        deltaApplier.Apply(basisStream, new BinaryDeltaReader(deltaStream, new ConsoleProgressReporter()), newFileStream);
                    }
                }
            }
        }
    }
}